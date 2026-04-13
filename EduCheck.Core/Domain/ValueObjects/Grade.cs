using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record Grade : ValueObject
{
    public int Value { get; }
    public static readonly Grade Min = new(0);
    public static readonly Grade Max = new(10);

    private Grade(int value) => Value = value;

    public static Result<Grade> Create(int value)
    {
        if (value < Min.Value || value > Max.Value)
            return Result.Failure<Grade>("Grade.Invalid", $"Оценка должна быть от {Min.Value} до {Max.Value}.");
        return new Grade(value);
    }
}
