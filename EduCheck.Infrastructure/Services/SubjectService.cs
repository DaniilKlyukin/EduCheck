using EduCheck.Core.Entities;
using EduCheck.Core.Interfaces;
using EduCheck.Core.ValueObjects;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduCheck.Infrastructure.Services;

public class SubjectService(
    IDbContextFactory<AppDbContext> dbFactory,
    ILogger<SubjectService> logger) : ISubjectService
{
    public async Task AddAssignmentAsync(Guid subjectId, string title, DateTime deadline)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == subjectId)
            ?? throw new Exception("Subject not found");

        subject.AddAssignment(title, deadline);

        await db.SaveChangesAsync();
    }

    public async Task AddTargetGroupAsync(Guid subjectId, string groupName)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var subject = await db.Subjects.Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Id == subjectId)
            ?? throw new Exception("Subject not found");

        subject.AddTargetGroup(groupName);
        await db.SaveChangesAsync();
    }

    public async Task<Guid> CreateSubjectAsync(string title, int semester)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var titleVo = new SubjectTitle(title);

        var subject = new Subject(titleVo, semester);
        db.Subjects.Add(subject);
        await db.SaveChangesAsync();
        return subject.Id;
    }

    public async Task DeleteAssignmentAsync(Guid subjectId, Guid assignmentId)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject != null)
        {
            subject.RemoveAssignment(assignmentId);
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteSubjectAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var subject = await db.Subjects.FindAsync(id);
        if (subject != null)
        {
            db.Subjects.Remove(subject);
            await db.SaveChangesAsync();
        }
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

        var subject = await db.Subjects
            .AsNoTracking()
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Assignments.Any(a => a.Id == assignmentId));

        if (subject == null || subject.TargetGroups.Count == 0)
            return [];

        var targetGroupNames = subject.TargetGroups
            .Select(tg => tg.GroupName)
            .ToList();

        var submittedStudentIds = await db.Submissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId)
            .Select(s => s.StudentId)
            .ToListAsync();

        return await db.Students
            .AsNoTracking()
            .Where(s => targetGroupNames.Contains(s.Group.Value))
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

    public async Task RemoveTargetGroupAsync(Guid subjectId, Guid targetGroupId)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects
            .Include(s => s.TargetGroups)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null) return;

        subject.RemoveTargetGroup(targetGroupId);

        await db.SaveChangesAsync();
    }

    public async Task UpdateAssignmentAsync(Guid subjectId, Guid assignmentId, string title, DateTime deadline)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects
            .Include(s => s.Assignments)
            .FirstOrDefaultAsync(s => s.Id == subjectId)
            ?? throw new Exception("Subject not found");

        var assignment = subject.Assignments.FirstOrDefault(a => a.Id == assignmentId)
            ?? throw new Exception("Assignment not found");

        assignment.Update(title, deadline);
        await db.SaveChangesAsync();
    }

    public async Task UpdateSubjectAsync(Guid id, string title, int semester)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        var subject = await db.Subjects.FindAsync(id)
            ?? throw new Exception("Subject not found");

        var titleVo = new SubjectTitle(title);

        subject.Update(titleVo, semester);

        await db.SaveChangesAsync();
    }
}
