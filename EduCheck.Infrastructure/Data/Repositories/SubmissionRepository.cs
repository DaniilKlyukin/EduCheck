using EduCheck.Application.DTOs;
using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Data.Repositories;

public class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task<Result<SubmissionAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var submission = await db.Submissions
                .Include(s => s.History)
                .Include(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (submission == null)
                return Result.Failure<SubmissionAggregate>("Submission.NotFound", "Работа не найдена.");

            return submission;
        }
        catch (Exception ex)
        {
            return Result.Failure<SubmissionAggregate>("Database.Error", ex.Message);
        }
    }

    public async Task<Result<bool>> ExistsAsync(Guid studentId, Guid assignmentId, FileHash hash, CancellationToken ct = default)
    {
        try
        {
            var exists = await db.Submissions
                .AnyAsync(s => s.StudentId == studentId &&
                               s.AssignmentId == assignmentId &&
                               s.History.Any(h => h.File.Hash == hash), ct);
            return exists;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>("Database.Error", ex.Message);
        }
    }

    public async Task<Result<List<Guid>>> GetStudentIdsByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        try
        {
            return await db.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .Select(s => s.StudentId)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Guid>>("Database.Error", ex.Message);
        }
    }

    public async Task<Result<List<SubmissionSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default)
    {
        try
        {
            var query = from submission in db.Submissions.AsNoTracking()
                        join student in db.Students.AsNoTracking() on submission.StudentId equals student.Id
                        from subject in db.Subjects.AsNoTracking()
                        from assignment in subject.Assignments
                        where assignment.Id == submission.AssignmentId
                        select new SubmissionSummaryDto(
                            submission.Id,
                            student.Name.Value,
                            student.Group.Value,
                            subject.Title.Value,
                            assignment.Title.Value,
                            submission.CurrentVersion == null ? null : submission.CurrentVersion.Value,
                            submission.Status,
                            submission.History.Any(h => h.IsLate)
                        );

            return await query.ToListAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubmissionSummaryDto>>("Database.Error", ex.Message);
        }
    }

    public async Task<Result<SubmissionHistory>> GetHistoryByIdAsync(Guid historyId, CancellationToken ct = default)
    {
        try
        {
            var history = await db.Set<SubmissionHistory>()
                .FirstOrDefaultAsync(h => h.Id == historyId, ct);

            if (history == null)
                return Result.Failure<SubmissionHistory>("History.NotFound", "Запись в истории не найдена.");

            return history;
        }
        catch (Exception ex)
        {
            return Result.Failure<SubmissionHistory>("Database.Error", ex.Message);
        }
    }

    public async Task<Result> AddAsync(SubmissionAggregate submission, CancellationToken ct = default)
    {
        try
        {
            await db.Submissions.AddAsync(submission, ct);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Database.Error", ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(SubmissionAggregate submission, CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Database.Error", ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var submission = await db.Submissions.FindAsync([id], ct);
            if (submission == null)
                return Result.Failure("Submission.NotFound", "Работа не найдена.");

            db.Submissions.Remove(submission);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Database.Error", ex.Message);
        }
    }
}