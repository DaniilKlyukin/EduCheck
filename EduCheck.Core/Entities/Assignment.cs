namespace EduCheck.Core.Entities;

/// <summary>
/// Конкретная учебная задача (лабораторная работа, проект) в рамках предмета.
/// </summary>
public class Assignment
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    /// <summary>
    /// Заголовок задания (например, "Лабораторная работа №1").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Срок сдачи, после которого загрузки помечаются как IsLate.
    /// </summary>
    public DateTime Deadline { get; set; }
}