namespace EduCheck.Core.ValueObjects;

public record GroupName
{
    public string Value { get; }

    public GroupName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Название группы не может быть пустым.");

        Value = value.Trim().ToUpperInvariant();
    }

    public override string ToString() => Value;
}