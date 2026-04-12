namespace EduCheck.Core.Primitives;

public record DomainError(string Code, string Message)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);
}