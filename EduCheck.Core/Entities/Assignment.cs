using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Конкретная учебная задача (лабораторная работа, проект) в рамках предмета.
/// </summary>
public class Assignment
{
    public Guid Id { get; private set; }
    public Guid SubjectId { get; private set; }
    public AssignmentTitle Title { get; private set; }
    public DateTime Deadline { get; private set; }

    private Assignment() { } // Для EF

    internal Assignment(AssignmentTitle title, DateTime deadline)
    {
        Update(title, deadline);
    }

    public void Update(AssignmentTitle title, DateTime deadline)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Deadline = deadline.ToUniversalTime();
    }
}