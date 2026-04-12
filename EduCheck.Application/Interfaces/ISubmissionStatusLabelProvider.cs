using EduCheck.Core.Domain.Enums;

namespace EduCheck.Application.Interfaces;

/// <summary>
/// Отвечает за человекочитаемые названия статусов.
/// </summary>
public interface ISubmissionStatusLabelProvider
{
    string GetDisplayName(SubmissionStatus status);
}
