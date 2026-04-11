using EduCheck.Core.Interfaces;
using System.Text.RegularExpressions;

namespace EduCheck.Infrastructure.Services;

public class EmailParser : IEmailParser
{
    private static readonly Regex _subjectRegex =
        new Regex(@"^\[(?<subject>.+)\]\[(?<group>.+)\]\[(?<assignment>.+)\]$",
            RegexOptions.Compiled);

    public ParsedEmail? Parse(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject)) return null;

        var match = _subjectRegex.Match(subject);
        if (!match.Success) return null;

        return new ParsedEmail(
            match.Groups["subject"].Value.Trim(),
            match.Groups["group"].Value.Trim(),
            match.Groups["assignment"].Value.Trim());
    }
}