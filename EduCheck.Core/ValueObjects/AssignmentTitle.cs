namespace EduCheck.Core.ValueObjects;

public record AssignmentTitle
{
    public string Value { get; }

    public AssignmentTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Название задания не может быть пустым.");

        Value = value.Trim();
    }

    public virtual bool Equals(AssignmentTitle? other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;
}