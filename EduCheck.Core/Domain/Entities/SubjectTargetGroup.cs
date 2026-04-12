using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Entities;

/// <summary>
/// Целевая аудитория предмета
/// </summary>
public class SubjectTargetGroup : Entity
{
    public Guid SubjectId { get; private set; }
    public GroupName GroupName { get; private set; }

    private SubjectTargetGroup() { }

    internal SubjectTargetGroup(Guid subjectId, GroupName groupName)
        : base(Guid.NewGuid())
    {
        SubjectId = subjectId;
        GroupName = groupName;
    }
}