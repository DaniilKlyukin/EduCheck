using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            return Result.Failure<EmailAddress>("Email.Invalid", "Некорректный email.");

        return new EmailAddress(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}