using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Data.Repositories;

public class StudentRepository(AppDbContext db) : IStudentRepository
{
    public async Task<Result<StudentAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var student = await db.Students.FindAsync([id], ct);
        return student ?? Result.Failure<StudentAggregate>("Student.NotFound", "Студент не найден.");
    }

    public async Task<Result<StudentAggregate>> GetByEmailAsync(EmailAddress email, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Email == email, ct);
        return student ?? Result.Failure<StudentAggregate>("Student.NotFound", "Студент с таким email не найден.");
    }

    public async Task<Result<List<StudentAggregate>>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Students
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Result<List<StudentAggregate>>> GetByGroupsAsync(List<string> groupNames, CancellationToken ct = default)
    {
        return await db.Students
            .AsNoTracking()
            .Where(s => groupNames.Contains(s.Group.Value))
            .ToListAsync(ct);
    }

    public async Task<Result> AddAsync(StudentAggregate student, CancellationToken ct = default)
    {
        await db.Students.AddAsync(student, ct);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(StudentAggregate student, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}