using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record SubjectTitle : ValueObject
{
    public string Value { get; }
    private SubjectTitle(string value) => Value = value;

    public static Result<SubjectTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<SubjectTitle>("SubjectTitle.Empty", "Название предмета не может быть пустым.");
        return new SubjectTitle(value.Trim());
    }

    public virtual bool Equals(SubjectTitle? other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;
}
