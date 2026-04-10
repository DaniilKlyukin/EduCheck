using EduCheck.Core.Entities;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduCheck.Infrastructure.Services;

public class SubjectService(
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<SubjectService> logger) : ISubjectService
{
    public async Task AddAssignmentAsync(Guid subjectId, Assignment assignment)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects.FindAsync(subjectId);

        if (subject == null)
        {
            logger.LogWarning($"Предмет {subjectId} не найден!");
            return;
        }

        subject.Assignments.Add(assignment);
    }

    public async Task CreateSubjectAsync(Subject subject)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        await db.Subjects.AddAsync(subject);

        await db.SaveChangesAsync();
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var assignment = await db.Assignments.FindAsync(assignmentId);

        if (assignment == null)
        {
            logger.LogWarning($"Задача {assignmentId} не найдена!");
            return;
        }

        assignment.Subject.Assignments.Remove(assignment);

        await db.SaveChangesAsync();
    }

    public async Task DeleteSubjectAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects.FindAsync(id);

        if (subject == null)
        {
            logger.LogWarning($"Предмет {id} не найден!");
            return;
        }

        db.Subjects.Remove(subject);

        await db.SaveChangesAsync();
    }

    public async Task<List<Subject>> GetAllSubjectsAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Subjects.ToListAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Subjects.FindAsync(id);
    }
}