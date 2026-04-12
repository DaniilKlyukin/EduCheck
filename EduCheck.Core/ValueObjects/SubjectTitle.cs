namespace EduCheck.Core.ValueObjects;

public record SubjectTitle
{
    public string Value { get; }

    public SubjectTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Название предмета не может быть пустым.");

        Value = value.Trim();
    }

    public virtual bool Equals(SubjectTitle? other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;
}
