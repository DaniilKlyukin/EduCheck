namespace EduCheck.Core.Entities;

/// <summary>
/// Информация о студенте. Идентификация происходит преимущественно по Email.
/// </summary>
public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный адрес почты, используемый для сопоставления входящих писем.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}