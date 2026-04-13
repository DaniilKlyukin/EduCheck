using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record SubmissionVersion : ValueObject
{
    public int Value { get; }

    public static readonly SubmissionVersion Initial = new(1);

    private SubmissionVersion(int value) => Value = value;
    
    public static Result<SubmissionVersion> Create(int version)
    {
        if (version < Initial.Value)
            return Result.Failure<SubmissionVersion>("SubmissionVersion.Invalid", $"Версия не может быть меньше {Initial.Value}.");

        return new SubmissionVersion(version);
    }

    public SubmissionVersion Increment() => new(Value + 1);
}