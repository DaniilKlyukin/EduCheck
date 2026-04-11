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
            .Select(s => new SubmissionSummaryDto(
                s.Id,
                s.Student.Name,
                s.Student.Group,
                s.Assignment.Subject.Title,
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
                .ThenInclude(a => a.Subject)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task SubmitReviewAsync(Guid submissionId, Review review, SubmissionStatus newStatus)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var submission = await db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null) return;

        submission.Status = newStatus;

        review.Id = Guid.NewGuid();
        review.CheckedAt = DateTime.UtcNow;
        review.SubmissionId = submission.Id;
        db.Reviews.Add(review);

        await db.SaveChangesAsync();

        var body = $"Ваша работа по предмету {submission.Assignment.Subject.Title} проверена.\n" +
                   $"Статус: {statusLabelProvider.GetDisplayName(submission.Status)}\n" +
                   $"Оценка: {review.Grade}\n" +
                   $"Комментарий: {review.TeacherComment}";

        await emailService.SendFeedbackAsync(submission.Student.Email, "Результат проверки", body);
    }
}
