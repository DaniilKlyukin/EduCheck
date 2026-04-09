namespace EduCheck.Core.Entities;

/// <summary>
/// Учебная дисциплина (например, "Объектно-ориентированное программирование").
/// </summary>
public class Subject
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Номер семестра для фильтрации актуальных предметов.
    /// </summary>
    public int Semester { get; set; }

    /// <summary>
    /// Список заданий (лабораторных, курсовых), входящих в данный предмет.
    /// </summary>
    public List<Assignment> Assignments { get; set; } = new();
}