namespace EduCheck.Core.ValueObjects;

public record FileHash
{
    public string Value { get; }

    public FileHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Хеш файла не может быть пустым.");

        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}