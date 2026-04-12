using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.Events;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Aggregates;

/// <summary>
/// Учебная дисциплина (например, "Объектно-ориентированное программирование").
/// </summary>
public class SubjectAggregate : AggregateRoot
{
    public SubjectTitle Title { get; private set; }
    public int Semester { get; private set; }
    private readonly List<Assignment> _assignments = new();
    private readonly List<SubjectTargetGroup> _targetGroups = new();

    public IReadOnlyCollection<Assignment> Assignments => _assignments.AsReadOnly();
    public IReadOnlyCollection<SubjectTargetGroup> TargetGroups => _targetGroups.AsReadOnly();

    private SubjectAggregate() { }

    public static Result<SubjectAggregate> Create(SubjectTitle title, int semester)
    {
        if (semester <= 0)
            return Result.Failure<SubjectAggregate>("Subject.InvalidSemester", "Семестр должен быть больше 0.");

        var subject = new SubjectAggregate { Id = Guid.NewGuid(), Title = title, Semester = semester };
        subject.RaiseDomainEvent(new SubjectCreatedEvent(subject.Id, title.Value));
        return subject;
    }

    public Result Update(SubjectTitle title, int semester)
    {
        if (semester <= 0)
            return Result.Failure("Subject.InvalidSemester", "Семестр должен быть больше 0.");

        Title = title;
        Semester = semester;
        return Result.Success();
    }

    public Result AddAssignment(AssignmentTitle title, DateTime deadline)
    {
        if (deadline < DateTime.UtcNow)
            return Result.Failure("Assignment.InvalidDeadline", "Дедлайн не может быть в прошлом.");

        if (_assignments.Any(a => a.Title == title))
            return Result.Failure("Assignment.Duplicate", "Задание с таким названием уже существует.");

        var assignment = new Assignment(Id, title, deadline);
        _assignments.Add(assignment);

        RaiseDomainEvent(new AssignmentCreatedEvent(Id, assignment.Id));
        return Result.Success();
    }

    public Result UpdateAssignment(Guid assignmentId, AssignmentTitle title, DateTime deadline)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null)
            return Result.Failure("Assignment.NotFound", "Задание не найдено в этом предмете.");

        assignment.Update(title, deadline);
        return Result.Success();
    }

    public void RemoveAssignment(Guid assignmentId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment != null) _assignments.Remove(assignment);
    }

    public void AddTargetGroup(GroupName groupName)
    {
        if (_targetGroups.Any(g => g.GroupName == groupName)) return;
        _targetGroups.Add(new SubjectTargetGroup(Id, groupName));
    }

    public void RemoveTargetGroup(Guid targetGroupId)
    {
        var tg = _targetGroups.FirstOrDefault(g => g.Id == targetGroupId);
        if (tg != null) _targetGroups.Remove(tg);
    }
}