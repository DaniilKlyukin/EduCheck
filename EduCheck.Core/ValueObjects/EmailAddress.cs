namespace EduCheck.Core.ValueObjects;

public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new ArgumentException("Некорректный email адрес.", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}