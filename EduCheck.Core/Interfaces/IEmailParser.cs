namespace EduCheck.Core.Interfaces;

public record ParsedEmail(string SubjectTitle, int Semester, string Group, string AssignmentTitle);

public interface IEmailParser
{
    ParsedEmail? Parse(string subject);
}