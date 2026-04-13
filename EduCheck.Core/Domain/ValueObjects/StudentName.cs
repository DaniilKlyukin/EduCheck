using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record StudentName : ValueObject
{
    public string Value { get; }

    private StudentName(string Name) => Value = Name;

    public static Result<StudentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<StudentName>("StudentName.Empty", "Имя студента не может быть пустым.");

        return new StudentName(value.Trim());
    }
}