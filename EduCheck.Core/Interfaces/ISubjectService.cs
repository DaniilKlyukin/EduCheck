using EduCheck.Core.Entities;

namespace EduCheck.Core.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(Guid id);
    Task<List<Student>> GetDebtorsAsync(Guid assignmentId);

    Task<Guid> CreateSubjectAsync(string title, int semester);
    Task UpdateSubjectAsync(Guid id, string title, int semester);
    Task DeleteSubjectAsync(Guid id);

    Task AddAssignmentAsync(Guid subjectId, string title, DateTime deadline);
    Task UpdateAssignmentAsync(Guid subjectId, Guid assignmentId, string title, DateTime deadline);
    Task DeleteAssignmentAsync(Guid subjectId, Guid assignmentId);

    Task AddTargetGroupAsync(Guid subjectId, string groupName);
    Task RemoveTargetGroupAsync(Guid subjectId, Guid targetGroupId);
}
