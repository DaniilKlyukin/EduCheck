using EduCheck.Core.Domain.Enums;

namespace EduCheck.Web.Interfaces;

/// <summary>
/// Отвечает за визуальное представление статусов (CSS-классы).
/// </summary>
public interface ISubmissionStatusStyleProvider
{
    string GetBadgeClass(SubmissionStatus status);
}
