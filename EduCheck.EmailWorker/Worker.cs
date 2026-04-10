using EduCheck.Core.Entities;
using EduCheck.Core.Enums;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Cryptography;

namespace EduCheck.EmailWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Проверка почты в: {DateTimeOffset.Now}");

            try
            {
                await ProcessEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении цикла воркера");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessEmailsAsync(CancellationToken ct)
    {
        using var client = new ImapClient();
        var settings = _config.GetSection("EmailSettings");

        await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["ImapPort"]!), true, ct);

        await client.AuthenticateAsync(settings["Email"], settings["Password"], ct);

        await client.Inbox.OpenAsync(FolderAccess.ReadWrite, ct);

        var uids = await client.Inbox.SearchAsync(SearchQuery.NotSeen, ct);

        foreach (var uid in uids)
        {
            try
            {
                var message = await client.Inbox.GetMessageAsync(uid, ct);

                using var scope = _scopeFactory.CreateScope();

                var parser = scope.ServiceProvider.GetRequiredService<IEmailParser>();

                var parsed = parser.Parse(message.Subject);

                if (parsed == null)
                {
                    _logger.LogWarning($"Формат темы не распознан: {message.Subject}");
                }
                else
                {
                    await HandleMessageAsync(scope, message, parsed, ct);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обработке письма UID: {uid}");
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
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

        var mailbox = message.From.Mailboxes.FirstOrDefault();
        var email = mailbox?.Address;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning($"Почта не найдена: {mailbox?.Name}");
            return;
        }

        var student = await db.Students.FirstOrDefaultAsync(s => s.Email == email, ct);

        if (student == null)
        {
            student = new Student 
            { 
                Id = Guid.NewGuid(), 
                Email = email, 
                Name = message.From.Mailboxes.First().Name, 
                Group = parsed.Group
            };

            db.Students.Add(student);

            _logger.LogInformation($"Создан новый студент: {student.Name} {student.Email}");
        }

        var assignment = await db.Assignments
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a =>
                a.Title == parsed.AssignmentTitle &&
                a.Subject.Title == parsed.SubjectTitle,
                ct);

        if (assignment == null)
        {
            _logger.LogWarning("Задание не найдено!");
            return;
        }

        foreach (var attachment in message.Attachments.OfType<MimePart>())
        {
            using var ms = new MemoryStream();

            await attachment.Content.DecodeToAsync(ms, ct);
            var hash = GetHash(ms.ToArray());

            if (await db.SubmissionHistory.AnyAsync(h => h.FileHash == hash, ct)) continue;

            var analysisResult = "Файл не является архивом или не поддерживается.";
            if (attachment.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var analyzer = scope.ServiceProvider.GetRequiredService<ICodeAnalyzer>();
                ms.Position = 0;
                analysisResult = await analyzer.AnalyzeZipAsync(ms, ct);
            }

            var submission = await db.Submissions
                .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.AssignmentId == assignment.Id, ct);

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

            var storagePath = await storage.UploadAsync(
                ms,
                attachment.FileName,
                attachment.ContentType.MimeType,
                ct);

            var history = new SubmissionHistory
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                Version = submission.CurrentVersion,
                FileName = attachment.FileName,
                FileStoragePath = storagePath,
                FileHash = hash,
                ReceivedAt = DateTime.UtcNow,
                IsLate = isLate,
                AnalysisResult = analysisResult
            };

            db.SubmissionHistory.Add(history);
        }

        await db.SaveChangesAsync(ct);
    }

    private string GetHash(byte[] data)
    {
        return Convert.ToHexString(SHA256.HashData(data));
    }
}
