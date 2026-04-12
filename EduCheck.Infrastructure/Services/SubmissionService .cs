using EduCheck.Core.DTOs;
using EduCheck.Core.Entities;
using EduCheck.Core.Enums;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Services;

public class SubmissionService(
    IDbContextFactory<AppDbContext> dbFactory,
    IEmailService emailService,
    IFileStorage fileStorage,
    ISubmissionStatusLabelProvider statusLabelProvider) : ISubmissionService
{
    public async Task DeleteSubmissionAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var submission = await db.Submissions.FindAsync(id);

        if (submission == null)
            return;

        db.Submissions.Remove(submission);

        await db.SaveChangesAsync();
    }

    public async Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Submissions
            .AsNoTracking()
            .Select(s => new SubmissionSummaryDto(
                s.Id,
                s.Student.Name,
                s.Student.Group.Value,
                db.Subjects
                    .Where(sub => sub.Assignments.Any(a => a.Id == s.AssignmentId))
                    .Select(sub => sub.Title.Value)
                    .FirstOrDefault() ?? "Unknown Subject",
                s.Assignment.Title,
                s.CurrentVersion,
                s.Status,
                s.HasLateUpload
            )).ToListAsync();
    }

    public async Task<string> GetDownloadUrlAsync(Guid historyId)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var history = await db.SubmissionHistory.FindAsync(historyId);

        if (history == null)
            throw new Exception("История не найдена!");

        return await fileStorage.GetDownloadUrlAsync(history.FileStoragePath, history.FileName);
    }

    public async Task<Submission?> GetSubmissionByIdAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Submissions
            .Include(s => s.History)
            .Include(s => s.Reviews)
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task SubmitReviewAsync(Guid submissionId, int? grade, string? comment, SubmissionStatus newStatus)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var submission = await db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Reviews)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new Exception("Submission not found");

        var subjectTitle = await db.Subjects
            .Where(sub => sub.Assignments.Any(a => a.Id == submission.AssignmentId))
            .Select(sub => sub.Title.Value)
            .FirstOrDefaultAsync() ?? "Дисциплина";

        submission.ReviewSubmission(grade, comment, newStatus);

        await db.SaveChangesAsync();

        var body = $"Ваша работа по предмету {subjectTitle} проверена.\n" +
                   $"Статус: {statusLabelProvider.GetDisplayName(submission.Status)}\n" +
                   $"Оценка: {grade?.ToString() ?? "без оценки"}\n" +
                   $"Комментарий: {comment}";

        await emailService.SendFeedbackAsync(submission.Student.Email.Value, "Результат проверки", body);
    }
}
