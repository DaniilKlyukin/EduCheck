using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Aggregates;

public class StudentAggregate : AggregateRoot
{
    public string Name { get; private set; }
    public GroupName Group { get; private set; }
    public EmailAddress Email { get; private set; }

    private StudentAggregate() { }

    public static Result<StudentAggregate> Create(string name, GroupName group, EmailAddress email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<StudentAggregate>("Student.NameEmpty", "Имя студента обязательно.");

        return new StudentAggregate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Group = group,
            Email = email
        };
    }
}