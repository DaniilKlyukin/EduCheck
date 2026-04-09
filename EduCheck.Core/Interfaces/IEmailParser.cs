namespace EduCheck.Core.Interfaces;

public record ParsedEmail(string SubjectTitle, string Group, string AssignmentTitle);

public interface IEmailParser
{
    ParsedEmail? Parse(string subject);
}