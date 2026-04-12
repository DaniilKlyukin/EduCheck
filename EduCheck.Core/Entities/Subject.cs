using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Учебная дисциплина (например, "Объектно-ориентированное программирование").
/// </summary>
public class Subject
{
    public Guid Id { get; private set; }
    public SubjectTitle Title { get; private set; }
    public int Semester { get; private set; }

    private readonly List<Assignment> _assignments = new();
    public IReadOnlyCollection<Assignment> Assignments => _assignments.AsReadOnly();

    private readonly List<SubjectTargetGroup> _targetGroups = new();
    public IReadOnlyCollection<SubjectTargetGroup> TargetGroups => _targetGroups.AsReadOnly();

    private Subject() { } // Для EF

    public Subject(SubjectTitle title, int semester)
    {
        Id = Guid.NewGuid();
        Update(title, semester);
    }

    public void Update(SubjectTitle title, int semester)
    {
        Title = title;
        Semester = semester > 0 ? semester : throw new ArgumentException("Invalid semester");
    }

    public void AddAssignment(string title, DateTime deadline)
    {
        _assignments.Add(new Assignment(title, deadline));
    }

    public void RemoveAssignment(Guid assignmentId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment != null) _assignments.Remove(assignment);
    }

    public void AddTargetGroup(string groupName)
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