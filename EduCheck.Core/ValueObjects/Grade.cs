namespace EduCheck.Core.ValueObjects;

public record Grade
{
    public int Value { get; }

    public Grade(int value)
    {
        if (value < 0 || value > 10)
            throw new ArgumentException("Оценка должна быть в диапазоне от 0 до 10.");

        Value = value;
    }

    public static explicit operator int(Grade grade) => grade.Value;
    public override string ToString() => Value.ToString();
}