using EduCheck.Core.Domain.Interfaces;
using EduCheck.Core.Primitives;
using System.Text.RegularExpressions;

namespace EduCheck.Infrastructure.Services;

public class EmailParser : IEmailParser
{
    private static readonly Regex _subjectRegex =
        new Regex(@"^\[(?<subject>.+)\]\[(?<semester>\d+)\]\[(?<group>.+)\]\[(?<assignment>.+)\]$",
            RegexOptions.Compiled);

    public Result<ParsedEmail> Parse(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure<ParsedEmail>("Subject.Empty", "Тема не может быть пустой.");

        var match = _subjectRegex.Match(subject);
        if (!match.Success)
            return Result.Failure<ParsedEmail>("Subject.Match", "Тема не соответствует шаблону.");

        if (!int.TryParse(match.Groups["semester"].Value, out var semester))
            return Result.Failure<ParsedEmail>("Semester.Parse", "Семестр не число.");

        return new ParsedEmail(
            match.Groups["subject"].Value.Trim(),
            semester,
            match.Groups["group"].Value.Trim(),
            match.Groups["assignment"].Value.Trim());
    }
}