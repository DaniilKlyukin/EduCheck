using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record FileHash : ValueObject
{
    public string Value { get; }
    private FileHash(string value) => Value = value;

    public static Result<FileHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FileHash>("FileHash.Empty", "Хеш не может быть пустым.");

        return new FileHash(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}