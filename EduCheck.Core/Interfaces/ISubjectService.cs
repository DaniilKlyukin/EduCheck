using EduCheck.Core.Entities;

namespace EduCheck.Core.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(Guid id);
    Task<List<Student>> GetDebtorsAsync(Guid assignmentId);
    Task CreateSubjectAsync(Subject subject);
    Task UpdateSubjectAsync(Subject subject);
    Task DeleteSubjectAsync(Guid id);
    Task AddAssignmentAsync(Guid subjectId, Assignment assignment);
    Task DeleteAssignmentAsync(Guid assignmentId);
    Task UpdateAssignmentAsync(Assignment assignment);
    Task AddTargetGroupAsync(Guid subjectId, string groupName);
    Task RemoveTargetGroupAsync(Guid id);
}
