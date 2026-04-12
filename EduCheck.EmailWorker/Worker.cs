using EduCheck.Core.Contracts;
using EduCheck.Core.Entities;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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
                logger.LogWarning($"Ошибка внешней службы. Попытка {retryCount}. Ждем {time.TotalSeconds}с. Ошибка: {ex.Message}");
            });

    private readonly long _maxFileSizeBytes = config.GetValue("EmailSettings:MaxAttachmentSizeMb", 25) * 1024 * 1024;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = config.GetValue("EmailSettings:CheckIntervalSeconds", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
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

        await client.ConnectAsync(settings["ImapServer"], settings.GetValue("ImapPort", 993), true, ct);
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

    private async Task HandleMessageAsync(IServiceScope scope, MimeMessage message, ParsedEmail parsed, CancellationToken ct)
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync(ct);

        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var analyzer = scope.ServiceProvider.GetRequiredService<ICodeAnalyzer>();
        var aiReviewer = scope.ServiceProvider.GetRequiredService<IAiCodeReviewer>();

        var mailbox = message.From.Mailboxes.FirstOrDefault();
        if (string.IsNullOrEmpty(mailbox?.Address)) return;

        var student = await studentService.GetOrCreateStudentAsync(mailbox.Name ?? mailbox.Address, parsed.Group, mailbox.Address);

        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s =>
                s.Title.Value == parsed.SubjectTitle &&
                s.Semester == parsed.Semester, ct);

        var assignment = subject?.Assignments.FirstOrDefault(a => a.Title == parsed.AssignmentTitle);

        if (assignment == null)
        {
            logger.LogWarning($"Задание '{parsed.AssignmentTitle}' не найдено в предмете '{parsed.SubjectTitle}'");
            return;
        }

        var archiveInfo = await CreateFinalArchiveAsync(message, ct);
        if (archiveInfo == null) return;

        var hash = GetHash(archiveInfo.FinalFileBytes);

        var submission = await db.Submissions
            .Include(s => s.History)
            .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == assignment.Id, ct);

        if (submission == null)
        {
            submission = new Submission(student.Id, assignment.Id);
            db.Submissions.Add(submission);
        }

        if (submission.History.Any(h => h.FileHash == hash))
        {
            logger.LogInformation("Эта версия файла уже была загружена ранее.");
            return;
        }

        var analysisResult = await analyzer.AnalyzeZipAsync(new MemoryStream(archiveInfo.FinalFileBytes), ct);
        string aiResult;
        try
        {
            var allCodeText = await ExtractAllCodeToStringAsync(archiveInfo.FinalFileBytes, ct);
            aiResult = await aiReviewer.GetReviewAsync(allCodeText, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI Review failed");
            aiResult = "Ошибка вызова ИИ.";
        }

        var finalReport = $"### ROSLYN ANALYSIS\n{analysisResult}\n\n### AI REVIEW\n{aiResult}";

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var storagePath = await storage.UploadAsync(
                new MemoryStream(archiveInfo.FinalFileBytes),
                archiveInfo.FinalFileName,
                "application/zip",
                ct);

            submission.AddAttempt(
                archiveInfo.FinalFileName,
                storagePath,
                hash,
                "Работа поставлена в очередь на автоматический анализ...",
                assignment.Deadline);

            await db.SaveChangesAsync(ct);

            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var historyRecord = submission.History.First(h => h.FileHash == hash);

            await publishEndpoint.Publish(new AnalyzeSubmissionTask(historyRecord.Id), ct);

            logger.LogInformation($"[Worker] Письмо обработано и отправлено в RabbitMQ: {student.Name}");
        });
    }

    private string GetHash(byte[] data) => Convert.ToHexString(SHA256.HashData(data));

    private async Task<FinalArchiveInfo?> CreateFinalArchiveAsync(MimeMessage message, CancellationToken ct)
    {
        var relevantAttachments = message.Attachments
            .OfType<MimePart>()
            .Where(p => p.FileName != null && IsRelevantFile(p.FileName))
            .ToList();

        if (relevantAttachments.Count == 0)
        {
            logger.LogWarning("Письмо пропущено: нет вложений подходящих форматов.");
            return null;
        }

        long totalSize = 0;
        foreach (var part in relevantAttachments)
        {
            totalSize += part.Content?.Stream?.Length ?? 0;
        }

        if (totalSize > _maxFileSizeBytes)
        {
            logger.LogWarning($"Письмо от {message.From} пропущено: размер вложений ({totalSize / 1024 / 1024} МБ) превышает лимит {_maxFileSizeBytes / 1024 / 1024} МБ");
            return null;
        }

        if (relevantAttachments.Count == 1 && IsArchive(relevantAttachments[0].FileName!))
        {
            using var ms = new MemoryStream();
            await relevantAttachments[0].Content.DecodeToAsync(ms, ct);
            return new FinalArchiveInfo(ms.ToArray(), relevantAttachments[0].FileName!);
        }

        var zipBytes = await CreateZipFromAttachmentsAsync(relevantAttachments, ct);
        return new FinalArchiveInfo(zipBytes, $"submission_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
    }

    private async Task<byte[]> CreateZipFromAttachmentsAsync(List<MimePart> parts, CancellationToken ct)
    {
        using var outStream = new MemoryStream();
        using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
        {
            foreach (var part in parts)
            {
                var entry = archive.CreateEntry(part.FileName ?? Guid.NewGuid().ToString(), CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                await part.Content.DecodeToAsync(entryStream, ct);
            }
        }
        return outStream.ToArray();
    }

    private async Task<string> ExtractAllCodeToStringAsync(byte[] zipBytes, CancellationToken ct)
    {
        var sb = new StringBuilder();
        using var archive = new ZipArchive(new MemoryStream(zipBytes));
        var codeExtensions = new[] { ".cs" };

        foreach (var entry in archive.Entries.Where(e => codeExtensions.Contains(Path.GetExtension(e.Name))))
        {
            using var reader = new StreamReader(entry.Open());
            sb.AppendLine($"FILE: {entry.FullName}\n{await reader.ReadToEndAsync(ct)}\n");
        }
        return sb.ToString();
    }

    private bool IsRelevantFile(string name) =>
        new[] { ".zip", ".cs", ".rar", ".7z" }.Contains(Path.GetExtension(name).ToLower());

    private bool IsArchive(string name) =>
        new[] { ".zip", ".rar", ".7z" }.Contains(Path.GetExtension(name).ToLower());
}