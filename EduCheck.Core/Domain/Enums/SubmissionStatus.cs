namespace EduCheck.Core.Domain.Enums;

/// <summary>
/// Определяет жизненный цикл проверки работы преподавателем.
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Новая работа, ждет ИИ и Roslyn.
    /// </summary>
    PendingAnalysis,

    /// <summary>
    /// Анализ завершен, ждет преподавателя.
    /// </summary>
    New,

    /// <summary>
    /// Преподаватель взял работу в работу.
    /// </summary>
    InReview,

    /// <summary>
    /// Работа проверена, но требует исправлений со стороны студента.
    /// </summary>
    UpdateRequired,

    /// <summary>
    /// Работа успешно защищена и закрыта для правок.
    /// </summary>
    Accepted
}