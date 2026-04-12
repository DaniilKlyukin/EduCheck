using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface IStudentService
{
    Task<Result<List<StudentAggregate>>> GetAllStudentsAsync();
    Task<Result<StudentAggregate>> GetStudentByIdAsync(Guid id);

    Task<Result<StudentAggregate>> GetOrCreateStudentAsync(string name, string group, string email);
}