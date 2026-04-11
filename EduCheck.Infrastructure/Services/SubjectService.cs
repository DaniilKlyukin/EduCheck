using EduCheck.Core.Entities;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static SharpCompress.Compressors.Filters.BranchExecFilter;

namespace EduCheck.Infrastructure.Services;

public class SubjectService(
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<SubjectService> logger) : ISubjectService
{
    public async Task AddAssignmentAsync(Guid subjectId, Assignment assignment)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
        {
            logger.LogWarning($"Предмет {subjectId} не найден!");
            return;
        }

        subject.Assignments.Add(assignment);
        await db.SaveChangesAsync();
    }

    public async Task AddTargetGroupAsync(Guid subjectId, string groupName)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null)
        {
            logger.LogWarning($"Предмет {subjectId} не найден!");
            return;
        }

        subject.TargetGroups.Add(
            new SubjectTargetGroup
            {
                Id = Guid.NewGuid(),
                GroupName = groupName,
                SubjectId = subjectId,
                Subject = subject
            });

        await db.SaveChangesAsync();
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

        var assignment = await db.Assignments
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

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

        return await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Student>> GetDebtorsAsync(Guid assignmentId)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var assignment = await db.Assignments
            .Include(a => a.Subject)
            .ThenInclude(s => s.TargetGroups)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment == null || assignment.Subject.TargetGroups.Count == 0)
            return new List<Student>();

        var targetGroupNames = assignment.Subject.TargetGroups
            .Select(tg => tg.GroupName)
            .ToList();

        var submittedStudentIds = await db.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Select(s => s.StudentId)
            .ToListAsync();

        return await db.Students
            .Where(s => targetGroupNames.Contains(s.Group))
            .Where(s => !submittedStudentIds.Contains(s.Id))
            .OrderBy(s => s.Group)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Subjects
            .Include(s => s.Assignments)
            .Include(s => s.TargetGroups)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task RemoveTargetGroupAsync(Guid targetGroupId)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var targetGroup = await db.TargetGroups
            .Include(tg => tg.Subject)
            .FirstOrDefaultAsync(tg => tg.Id == targetGroupId);

        if (targetGroup == null)
        {
            logger.LogWarning($"Целевая группа {targetGroupId} не найдена!");
            return;
        }

        targetGroup.Subject.TargetGroups.Remove(targetGroup);

        await db.SaveChangesAsync();
    }

    public async Task UpdateAssignmentAsync(Assignment assignment)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        db.Assignments.Update(assignment);

        await db.SaveChangesAsync();
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        db.Subjects.Update(subject);

        await db.SaveChangesAsync();
    }
}
