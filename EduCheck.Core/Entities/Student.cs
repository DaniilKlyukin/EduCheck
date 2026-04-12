using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

public class Student
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupName Group { get; private set; }
    public EmailAddress Email { get; private set; }

    // Для EF Core
    private Student() { }

    public Student(string name, GroupName group, EmailAddress email)
    {
        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        Group = group ?? throw new ArgumentNullException(nameof(group));
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }
}