using EduCheck.Application.DTOs;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface ISubmissionRepository
{
    Task<Result<SubmissionAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Result<bool>> ExistsAsync(Guid studentId, Guid assignmentId, FileHash hash, CancellationToken ct = default);

    Task<Result<List<Guid>>> GetStudentIdsByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);

    Task<Result<List<SubmissionSummaryDto>>> GetAllSummariesAsync(CancellationToken ct = default);

    Task<Result> AddAsync(SubmissionAggregate submission, CancellationToken ct = default);
    Task<Result> UpdateAsync(SubmissionAggregate submission, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result<SubmissionHistory>> GetHistoryByIdAsync(Guid historyId, CancellationToken ct = default);
}
