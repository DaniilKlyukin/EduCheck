using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Services;

public class StudentService(IStudentRepository studentRepository) : IStudentService
{
    public async Task<Result<List<StudentAggregate>>> GetAllStudentsAsync()
    {
        return await studentRepository.GetAllAsync();
    }

    public async Task<Result<StudentAggregate>> GetOrCreateStudentAsync(string name, string group, string email)
    {
        var nameRes = StudentName.Create(name);
        var emailRes = EmailAddress.Create(email);
        var groupRes = GroupName.Create(group);

        if (nameRes.IsFailure) return nameRes.Error;
        if (emailRes.IsFailure) return emailRes.Error;
        if (groupRes.IsFailure) return groupRes.Error;

        var studentRes = await studentRepository.GetByEmailAsync(emailRes.Value);

        if (studentRes.IsFailure)
        {
            var createRes = StudentAggregate.Create(nameRes.Value, groupRes.Value, emailRes.Value);
            if (createRes.IsFailure) return createRes.Error;

            await studentRepository.AddAsync(createRes.Value);
            return createRes.Value;
        }

        return studentRes.Value;
    }

    public async Task<Result<StudentAggregate>> GetStudentByIdAsync(Guid id)
    {
        return await studentRepository.GetByIdAsync(id);
    }
}