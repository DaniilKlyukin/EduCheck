namespace EduCheck.Core.Entities;

/// <summary>
/// Результат проверки работы преподавателем.
/// </summary>
public class Review
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }

    public Submission Submission { get; set; } = null!;

    /// <summary>
    /// Указывает, какая именно версия работы была оценена данным ревью.
    /// </summary>
    public int SubmissionVersion { get; set; }

    /// <summary>
    /// Оценка по установленной шкале (может быть null, если оставлен только комментарий).
    /// </summary>
    public int? Grade { get; set; }

    /// <summary>
    /// Обратная связь от преподавателя студенту.
    /// </summary>
    public string? TeacherComment { get; set; }

    public DateTime CheckedAt { get; set; }
}