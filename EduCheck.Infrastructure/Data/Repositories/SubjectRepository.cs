using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Data.Repositories;

public class SubjectRepository(AppDbContext db) : ISubjectRepository
{
    public async Task<Result<SubjectAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (subject == null)
            return Result.Failure<SubjectAggregate>("Subject.NotFound", "Предмет не найден.");

        return subject;
    }

    public async Task<Result<SubjectAggregate>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Assignments.Any(a => a.Id == assignmentId), ct);

        return subject ?? Result.Failure<SubjectAggregate>("Subject.NotFound", "Предмет для данного задания не найден.");
    }

    public async Task<Result<List<SubjectAggregate>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.Subjects
                .AsNoTracking()
                .Include(s => s.Assignments)
                .Include(s => s.TargetGroups)
                .AsSplitQuery()
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SubjectAggregate>>("Database.Error", ex.Message);
        }
    }

    public async Task<Result> AddAsync(SubjectAggregate subject, CancellationToken ct = default)
    {
        await db.Subjects.AddAsync(subject, ct);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(SubjectAggregate subject, CancellationToken ct = default)
    {
        try
        {
            var entry = db.Entry(subject);
            if (entry.State == EntityState.Detached)
            {
                db.Subjects.Update(subject);
            }

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("Database.Concurrency", "Данные были изменены другим пользователем. Пожалуйста, обновите страницу.");
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
            var subject = await db.Subjects.FindAsync([id], ct);
            if (subject == null) return Result.Failure("Subject.NotFound", "Предмет не найден");

            db.Subjects.Remove(subject);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Database.Error", ex.Message);
        }
    }
}
