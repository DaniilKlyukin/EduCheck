using EduCheck.Core.Entities;
using EduCheck.Core.Enums;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Minio.DataModel;
using Polly;
using Polly.Retry;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace EduCheck.EmailWorker;

public class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration config) : BackgroundService
{
    private record FinalArchiveInfo(byte[] FinalFileBytes, string FinalFileName);

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (ex, time, retryCount, context) =>
            {
                Console.WriteLine($"Ошибка внешней службы. Попытка {retryCount}. Ждем {time.TotalSeconds}с. Ошибка: {ex.Message}");
            });

    private readonly long _maxFileSizeBytes = config.GetValue("EmailSettings:MaxAttachmentSizeMb", 25) * 1024 * 1024;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = config.GetValue("EmailSettings:CheckIntervalSeconds", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation($"Проверка почты в: {DateTimeOffset.Now}");

            try
            {
                await ProcessEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при выполнении цикла воркера");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessEmailsAsync(CancellationToken ct)
    {
        using var client = new ImapClient();
        var settings = config.GetSection("EmailSettings");

        var useSsl = settings.GetValue("UseSsl", true);
        await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["ImapPort"]!), useSsl, ct);

        await client.AuthenticateAsync(settings["Email"], settings["Password"], ct);
        await client.Inbox.OpenAsync(FolderAccess.ReadWrite, ct);

        var uids = await client.Inbox.SearchAsync(SearchQuery.NotSeen, ct);

        foreach (var uid in uids)
        {
            try
            {
                var message = await client.Inbox.GetMessageAsync(uid, ct);

                using var scope = scopeFactory.CreateScope();

                var parser = scope.ServiceProvider.GetRequiredService<IEmailParser>();

                var parsed = message.Subject != null ? parser.Parse(message.Subject) : null;

                if (parsed == null)
                {
                    logger.LogWarning($"Формат темы не распознан: {message.Subject}");
                }
                else
                {
                    await HandleMessageAsync(scope, message, parsed, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Ошибка при обработке письма UID: {uid}");
            }
            finally
            {
                await client.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, ct);
            }
        }

        await client.DisconnectAsync(true, ct);
    }

    private async Task HandleMessageAsync(
        IServiceScope scope,
        MimeMessage message,
        ParsedEmail parsed,
        CancellationToken ct)
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync(ct);

        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var analyzer = scope.ServiceProvider.GetRequiredService<ICodeAnalyzer>();
        var aiReviewer = scope.ServiceProvider.GetRequiredService<IAiCodeReviewer>();

        var mailbox = message.From.Mailboxes.FirstOrDefault();
        var email = mailbox?.Address;

        if (string.IsNullOrEmpty(email))
        {
            logger.LogWarning($"Почта не найдена: {mailbox?.Name}");
            return;
        }

        var name = message.From.Mailboxes.First().Name ?? $"Студент {email}";

        var (student, assignment) = await _retryPolicy.ExecuteAsync(async () =>
        {
            var s = await studentService.GetOrCreateStudentAsync(name, parsed.Group, email);
            var a = await db.Assignments
                .Include(a => a.Subject)
                .FirstOrDefaultAsync(a =>
                    a.Title == parsed.AssignmentTitle &&
                    a.Subject.Title == parsed.SubjectTitle, ct);
            return (s, a);
        });

        if (assignment == null)
        {
            logger.LogWarning($"Задание '{parsed.AssignmentTitle}' не найдено для предмета '{parsed.SubjectTitle}'");
            return;
        }

        var archiveInfo = await CreateFinalArchiveAsync(message, ct);

        if (archiveInfo == null)
            return;

        var hash = GetHash(archiveInfo.FinalFileBytes);

        if (await db.SubmissionHistory.AnyAsync(h => h.FileHash == hash, ct))
        {
            logger.LogInformation("Такая версия файла уже существует, пропускаем.");
            return;
        }

        var analysisResult = await analyzer.AnalyzeZipAsync(
            new MemoryStream(archiveInfo.FinalFileBytes),
            ct);

        var aiResult = "ИИ-анализ пропущен.";
        try
        {
            var allCodeText = await ExtractAllCodeToStringAsync(archiveInfo.FinalFileBytes, ct);
            aiResult = await aiReviewer.GetReviewAsync(allCodeText, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI Review failed");
        }

        var finalReport = $"""
            ### ROSLYN STATIC ANALYSIS
            {analysisResult}
    
            ### AI TEACHER REVIEW
            {aiResult}
        """;

        var submission = await db.Submissions
            .FirstOrDefaultAsync(s => s.StudentId == student.Id &&
                                      s.AssignmentId == assignment.Id,
                                      ct);

        if (submission == null)
        {
            submission = new Submission
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                AssignmentId = assignment.Id,
                Status = SubmissionStatus.New
            };
            db.Submissions.Add(submission);
        }

        submission.CurrentVersion++;
        submission.LastActivityAt = DateTime.UtcNow;
        var isLate = DateTime.UtcNow > assignment.Deadline;
        if (isLate) submission.HasLateUpload = true;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var storagePath = await storage.UploadAsync(
                new MemoryStream(archiveInfo.FinalFileBytes),
                archiveInfo.FinalFileName,
                "application/zip",
                ct);

            var history = new SubmissionHistory
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                Version = submission.CurrentVersion,
                FileName = archiveInfo.FinalFileName,
                FileStoragePath = storagePath,
                FileHash = hash,
                ReceivedAt = DateTime.UtcNow,
                IsLate = isLate,
                AnalysisResult = finalReport
            };

            db.SubmissionHistory.Add(history);
            await db.SaveChangesAsync(ct);
        });
    }

    private async Task<FinalArchiveInfo?> CreateFinalArchiveAsync(MimeMessage message, CancellationToken ct)
    {
        var relevantAttachments = message.Attachments
            .OfType<MimePart>()
            .Where(p => p.FileName != null && IsRelevantFile(p.FileName))
            .ToList();

        if (relevantAttachments.Count == 0)
        {
            logger.LogWarning($"Нет корректных прикрепленных файлов");
            return null;
        }

        if (relevantAttachments.Sum(p => p.Content?.Stream?.Length ?? 0) > _maxFileSizeBytes)
        {
            logger.LogWarning($"Файл слишком большой.");
            return null;
        }

        if (relevantAttachments.Count == 1 && IsArchive(relevantAttachments[0].FileName!))
        {
            var content = relevantAttachments[0].Content;

            if (content == null)
            {
                logger.LogWarning($"Отправленный архив '{relevantAttachments[0].FileName}' пуст");
                return null;
            }

            using var ms = new MemoryStream();
            await content.DecodeToAsync(ms, ct);

            return new FinalArchiveInfo(ms.ToArray(), relevantAttachments[0].FileName!);
        }
        else
        {
            return new FinalArchiveInfo(
                await CreateZipFromAttachmentsAsync(relevantAttachments, ct),
                $"submission_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip"
                );
        }
    }

    private string GetHash(byte[] data)
    {
        return Convert.ToHexString(SHA256.HashData(data));
    }

    private bool IsRelevantFile(string fileName)
    {
        if (fileName == null)
            return false;

        var ext = Path.GetExtension(fileName).ToLower();
        var allowedExts = config.GetSection("EmailSettings:AllowedExtensions").Get<string[]>()
                          ?? [".zip", ".rar", ".7z", ".tar", ".gz", ".cs", ".cpp", ".h", ".py", ".txt", ".pdf", ".java", ".js"];

        if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            return false;

        return true;
    }

    private bool IsArchive(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext == ".zip" || ext == ".rar" || ext == ".7z" || ext == ".tar";
    }

    private async Task<byte[]> CreateZipFromAttachmentsAsync(List<MimePart> parts, CancellationToken ct)
    {
        using var outStream = new MemoryStream();
        using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
        {
            var size = 0L;

            foreach (var part in parts)
            {
                if (part.Content == null)
                    continue;

                size += part.Content.Stream?.Length ?? 0;

                if (size > _maxFileSizeBytes)
                    break;

                var fileName = part.FileName ?? Guid.NewGuid().ToString();

                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = await entry.OpenAsync();
                await part.Content.DecodeToAsync(entryStream, ct);
            }
        }

        return outStream.ToArray();
    }

    private async Task<string> ExtractAllCodeToStringAsync(byte[] zipBytes, CancellationToken ct)
    {
        var sb = new StringBuilder();
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms);

        var excludedFolders = config.GetSection("AnalysisSettings:ExcludedFolders").Get<string[]>() ?? [];
        var codeExtensions = config.GetSection("AnalysisSettings:CodeExtensions").Get<string[]>() ?? [".cs"];

        foreach (var entry in archive.Entries)
        {
            var entryPath = entry.FullName.Replace('\\', '/');

            var hasValidExt = codeExtensions.Any(ext => entryPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
            var isExcluded = excludedFolders.Any(folder => entryPath.Contains(folder, StringComparison.OrdinalIgnoreCase));

            if (!hasValidExt || isExcluded) continue;

            using var reader = new StreamReader(entry.Open());
            var code = await reader.ReadToEndAsync(ct);

            sb.AppendLine($"FILE: {entry.FullName}");
            sb.AppendLine(code);
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
