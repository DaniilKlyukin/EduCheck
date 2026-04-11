namespace EduCheck.Core.Entities;

/// <summary>
/// Целевая аудитория предмета
/// </summary>
public class SubjectTargetGroup
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public string GroupName { get; set; } = string.Empty;
}