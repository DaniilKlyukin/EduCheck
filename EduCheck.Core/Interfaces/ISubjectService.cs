using EduCheck.Core.Entities;

namespace EduCheck.Core.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(Guid id);
    Task CreateSubjectAsync(Subject subject);
    Task DeleteSubjectAsync(Guid id);
    Task AddAssignmentAsync(Guid subjectId, Assignment assignment);
    Task DeleteAssignmentAsync(Guid assignmentId);
}
