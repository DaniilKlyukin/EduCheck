using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface ISubjectService
{
    Task<Result<List<SubjectAggregate>>> GetAllSubjectsAsync();
    Task<Result<SubjectAggregate>> GetSubjectByIdAsync(Guid id);
    Task<Result<SubjectAggregate>> GetSubjectByAssignmentIdAsync(Guid assignmentId);
    Task<Result<List<StudentAggregate>>> GetDebtorsAsync(Guid assignmentId);

    Task<Result<Guid>> CreateSubjectAsync(string title, int semester);
    Task<Result> UpdateSubjectAsync(Guid id, string title, int semester);
    Task<Result> DeleteSubjectAsync(Guid id);

    Task<Result> AddAssignmentAsync(Guid subjectId, string title, DateTime deadline);
    Task<Result> UpdateAssignmentAsync(Guid subjectId, Guid assignmentId, string title, DateTime deadline);
    Task<Result> DeleteAssignmentAsync(Guid subjectId, Guid assignmentId);

    Task<Result> AddTargetGroupAsync(Guid subjectId, string groupName);
    Task<Result> RemoveTargetGroupAsync(Guid subjectId, Guid targetGroupId);
}
