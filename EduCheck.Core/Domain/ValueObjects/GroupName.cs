using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record GroupName : ValueObject
{
    public string Value { get; }

    private GroupName(string value) => Value = value;

    public static Result<GroupName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<GroupName>("GroupName.Empty", "Группа не может быть пустой.");

        return new GroupName(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
