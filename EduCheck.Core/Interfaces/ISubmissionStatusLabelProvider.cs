using EduCheck.Core.Enums;

namespace EduCheck.Core.Interfaces;

/// <summary>
/// Отвечает за человекочитаемые названия статусов.
/// </summary>
public interface ISubmissionStatusLabelProvider
{
    string GetDisplayName(SubmissionStatus status);
}
