using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface IStudentRepository
{
    Task<Result<StudentAggregate>> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Result<StudentAggregate>> GetByEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task<Result<List<StudentAggregate>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<List<StudentAggregate>>> GetByGroupsAsync(List<string> groupNames, CancellationToken ct = default);

    Task<Result> AddAsync(StudentAggregate student, CancellationToken ct = default);
    Task<Result> UpdateAsync(StudentAggregate student, CancellationToken ct = default);
}