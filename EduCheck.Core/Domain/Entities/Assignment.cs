using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Entities;

/// <summary>
/// Конкретная учебная задача (лабораторная работа, проект) в рамках предмета.
/// </summary>
public class Assignment : Entity
{
    public Guid SubjectId { get; private set; }
    public AssignmentTitle Title { get; private set; }
    public DateTime Deadline { get; private set; }

    private Assignment() { }

    internal Assignment(Guid subjectId, AssignmentTitle title, DateTime deadline)
        : base(Guid.NewGuid())
    {
        SubjectId = subjectId;
        Title = title;
        Deadline = deadline.ToUniversalTime();
    }

    internal void Update(AssignmentTitle title, DateTime deadline)
    {
        Title = title;
        Deadline = deadline.ToUniversalTime();
    }
}