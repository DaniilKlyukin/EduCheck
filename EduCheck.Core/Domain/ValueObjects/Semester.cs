using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record Semester : ValueObject
{
    public int Value { get; }
    public static readonly Semester Min = new(1);
    public static readonly Semester Max = new(20);

    private Semester(int value) => Value = value;

    public static Result<Semester> Create(int value)
    {
        if (value < Min.Value || value > Max.Value)
            return Result.Failure<Semester>("Semester.Invalid", $"Семестр должен быть от {Min.Value} до {Max.Value}.");

        return new Semester(value);
    }
}