using EduCheck.Core.Common;
using EduCheck.Core.DTOs;
using EduCheck.Core.Entities;
using EduCheck.Core.Enums;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IEmailService _emailService;

    public SubmissionService(IDbContextFactory<AppDbContext> dbFactory, IEmailService emailService)
    {
        _dbFactory = dbFactory;
        _emailService = emailService;
    }

    public async Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Submissions
            .Select(s => new SubmissionSummaryDto(
                s.Id,
                db.Students.First(st => st.Id == s.StudentId).Name,
                db.Students.First(st => st.Id == s.StudentId).Group,
                db.Assignments.Include(a => a.Subject).First(a => a.Id == s.AssignmentId).Subject.Title,
                db.Assignments.First(a => a.Id == s.AssignmentId).Title,
                s.CurrentVersion,
                s.Status,
                s.HasLateUpload
            )).ToListAsync();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(Guid id)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Submissions
            .Include(s => s.History)
            .Include(s => s.Student)
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Subject)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task SubmitReviewAsync(Guid submissionId, Review review, SubmissionStatus newStatus)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

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
                   $"Статус: {newStatus.GetDisplayName()}\n" +
                   $"Оценка: {review.Grade}\n" +
                   $"Комментарий: {review.TeacherComment}";

        await _emailService.SendFeedbackAsync(submission.Student.Email, "Результат проверки", body);
    }
}
