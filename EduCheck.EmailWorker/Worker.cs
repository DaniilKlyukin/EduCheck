using EduCheck.Application.Contracts;
using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Interfaces;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;
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
                logger.LogWarning($"Ошибка инфраструктуры. Попытка {retryCount}. Ждем {time.TotalSeconds}с. Ошибка: {ex.Message}");
            });

    private readonly long _maxFileSizeBytes = config.GetValue("EmailSettings:MaxAttachmentSizeMb", 25) * 1024 * 1024;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = config.GetValue("EmailSettings:CheckIntervalSeconds", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessEmailsAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Ошибка в цикле воркера"); }

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
            using var scope = scopeFactory.CreateScope();
            var parser = scope.ServiceProvider.GetRequiredService<IEmailParser>();

            var message = await client.Inbox.GetMessageAsync(uid, ct);
            var parseResult = parser.Parse(message.Subject ?? "");

            if (parseResult.IsFailure)
            {
                logger.LogWarning($"Тема письма {uid} не распознана: {message.Subject}. Ошибка: {parseResult.Error.Message}");
                await client.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, ct);
                continue;
            }

            var result = await HandleMessageInternalAsync(scope, message, parseResult.Value, ct);

            if (result.IsSuccess)
            {
                await client.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, ct);
            }
            else
            {
                logger.LogError($"Ошибка обработки письма {uid}: {result.Error.Message}");
            }
        }
        await client.DisconnectAsync(true, ct);
    }

    private async Task<Result> HandleMessageInternalAsync(IServiceScope scope, MimeMessage message, ParsedEmail parsed, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var mailbox = message.From.Mailboxes.FirstOrDefault();
        if (mailbox == null) return Result.Failure("Email.NoSender", "Отправитель не определен.");

        var studentRes = await studentService.GetOrCreateStudentAsync(mailbox.Name ?? mailbox.Address, parsed.Group, mailbox.Address);
        if (studentRes.IsFailure) return studentRes.Error;

        var subject = await db.Subjects.Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Title.Value == parsed.SubjectTitle && s.Semester.Value == parsed.Semester, ct);

        var assignment = subject?.Assignments.FirstOrDefault(a => a.Title.Value.Equals(parsed.AssignmentTitle, StringComparison.OrdinalIgnoreCase));
        if (assignment == null) return Result.Failure("Assignment.NotFound", $"Задание {parsed.AssignmentTitle} не найдено.");

        var archiveInfo = await CreateFinalArchiveAsync(message, ct);
        if (archiveInfo == null) return Result.Failure("Attachment.Empty", "Письмо не содержит подходящих файлов или превышен лимит размера.");

        var hashRes = FileHash.Create(Convert.ToHexString(SHA256.HashData(archiveInfo.FinalFileBytes)));
        if (hashRes.IsFailure) return hashRes.Error;

        var isDuplicate = await db.Submissions.AnyAsync(s =>
            s.StudentId == studentRes.Value.Id &&
            s.AssignmentId == assignment.Id &&
            s.History.Any(h => h.File.Hash == hashRes.Value), ct);

        if (isDuplicate) return Result.Success();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var uploadRes = await storage.UploadAsync(new MemoryStream(archiveInfo.FinalFileBytes), archiveInfo.FinalFileName, "application/zip", ct);
            if (uploadRes.IsFailure) return uploadRes.Error;

            var submission = await db.Submissions.Include(s => s.History)
                .FirstOrDefaultAsync(s => s.StudentId == studentRes.Value.Id && s.AssignmentId == assignment.Id, ct);

            if (submission == null)
            {
                submission = SubmissionAggregate.Create(studentRes.Value.Id, assignment.Id);
                db.Submissions.Add(submission);
            }

            var metadataVo = FileMetadata.Create(archiveInfo.FinalFileName, uploadRes.Value, hashRes.Value);

            if (metadataVo.IsFailure) return metadataVo.Error;

            var attemptRes = submission.AddAttempt(metadataVo.Value, assignment.Deadline);
            if (attemptRes.IsFailure) return attemptRes.Error;

            await db.SaveChangesAsync(ct);

            var historyRecord = submission.History.First(h => h.FileHash == hashRes.Value);
            await publishEndpoint.Publish(new AnalyzeSubmissionTask(historyRecord.Id), ct);

            return Result.Success();
        });
    }

    private async Task<FinalArchiveInfo?> CreateFinalArchiveAsync(MimeMessage message, CancellationToken ct)
    {
        var relevantAttachments = message.Attachments.OfType<MimePart>()
            .Where(p => p.FileName != null && IsRelevantFile(p.FileName)).ToList();

        if (relevantAttachments.Count == 0) return null;

        var totalSize = relevantAttachments.Sum(p => p.Content?.Stream?.Length ?? 0);
        if (totalSize > _maxFileSizeBytes)
        {
            logger.LogWarning($"Размер вложений ({totalSize / 1024 / 1024} МБ) превышает лимит.");
            return null;
        }

        if (relevantAttachments.Count == 1 && IsArchive(relevantAttachments[0].FileName!))
        {
            using var ms = new MemoryStream();
            await relevantAttachments[0].Content.DecodeToAsync(ms, ct);
            return new FinalArchiveInfo(ms.ToArray(), relevantAttachments[0].FileName!);
        }

        using var outStream = new MemoryStream();
        using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
        {
            foreach (var part in relevantAttachments)
            {
                var entry = archive.CreateEntry(part.FileName ?? Guid.NewGuid().ToString(), CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                await part.Content.DecodeToAsync(entryStream, ct);
            }
        }
        return new FinalArchiveInfo(outStream.ToArray(), $"submission_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
    }

    private bool IsRelevantFile(string name) =>
        new[] { ".zip", ".cs", ".rar", ".7z" }.Contains(Path.GetExtension(name).ToLower());

    private bool IsArchive(string name) =>
        new[] { ".zip", ".rar", ".7z" }.Contains(Path.GetExtension(name).ToLower());
}