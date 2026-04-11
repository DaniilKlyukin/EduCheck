using EduCheck.Core.Entities;

namespace EduCheck.Core.Interfaces;

public interface IStudentService
{
    Task<List<Student>> GetAllStudentsAsync();
    Task<Student?> GetStudentByIdAsync(Guid id);
    Task<Student> GetOrCreateStudentAsync(string name, string group, string email);
}