using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface ISubjectRepository
{
    Task<Result<SubjectAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Result<SubjectAggregate>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default);

    Task<Result<List<SubjectAggregate>>> GetAllAsync(CancellationToken ct = default);

    Task<Result> AddAsync(SubjectAggregate subject, CancellationToken ct = default);
    Task<Result> UpdateAsync(SubjectAggregate subject, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}