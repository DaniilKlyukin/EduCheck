using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record AssignmentTitle : ValueObject
{
    public string Value { get; }
    private AssignmentTitle(string value) => Value = value;

    public static Result<AssignmentTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<AssignmentTitle>("AssignmentTitle.Empty", "Название задания не может быть пустым.");
        return new AssignmentTitle(value.Trim());
    }

    public virtual bool Equals(AssignmentTitle? other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;
}