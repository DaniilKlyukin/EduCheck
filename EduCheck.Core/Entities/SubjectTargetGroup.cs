using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Целевая аудитория предмета
/// </summary>
public class SubjectTargetGroup
{
    public Guid Id { get; private set; }
    public Guid SubjectId { get; private set; }
    public GroupName GroupName { get; private set; }

    private SubjectTargetGroup() { }

    internal SubjectTargetGroup(Guid subjectId, GroupName groupName)
    {
        SubjectId = subjectId;
        GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
    }
}