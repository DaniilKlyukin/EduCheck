using System.ComponentModel.DataAnnotations;

namespace EduCheck.Core.Enums;

/// <summary>
/// Определяет жизненный цикл проверки работы преподавателем.
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Работа получена и ожидает первичного ознакомления.
    /// </summary>
    [Display(Name = "Новое")]
    New,

    /// <summary>
    /// Преподаватель взял работу в работу.
    /// </summary>
    [Display(Name = "На проверке")]
    InReview,

    /// <summary>
    /// Работа проверена, но требует исправлений со стороны студента.
    /// </summary>
    [Display(Name = "Требует исправлений")]
    UpdateRequired,

    /// <summary>
    /// Работа успешно защищена и закрыта для правок.
    /// </summary>
    [Display(Name = "Принято")]
    Accepted
}