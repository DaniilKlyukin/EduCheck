using EduCheck.Core.Entities;
using EduCheck.Core.Interfaces;
using EduCheck.Core.ValueObjects;
using EduCheck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduCheck.Infrastructure.Services;

public class StudentService(IDbContextFactory<AppDbContext> dbFactory) : IStudentService
{
    public async Task<List<Student>> GetAllStudentsAsync()
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Students.ToListAsync();
    }

    public async Task<Student> GetOrCreateStudentAsync(string name, string group, string email)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var emailVo = new EmailAddress(email);
        var groupVo = new GroupName(group);

        var student = await db.Students.FirstOrDefaultAsync(s => s.Email == emailVo);

        if (student == null)
        {
            student = new Student(name, groupVo, emailVo);
            db.Students.Add(student);
            await db.SaveChangesAsync();
        }

        return student;
    }

    public async Task<Student?> GetStudentByIdAsync(Guid id)
    {
        using var db = await dbFactory.CreateDbContextAsync();

        return await db.Students.FindAsync(id);
    }
}