using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Interfaces;

public record ParsedEmail(string SubjectTitle, int Semester, string Group, string AssignmentTitle);

public interface IEmailParser
{
    Result<ParsedEmail> Parse(string subject);
}