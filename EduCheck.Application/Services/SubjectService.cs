using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Services;

public class SubjectService(
    ISubjectRepository subjectRepository,
    ISubmissionRepository submissionRepository,
    IStudentRepository studentRepository) : ISubjectService
{
    public async Task<Result<Guid>> CreateSubjectAsync(string title, int semester)
    {
        var titleRes = SubjectTitle.Create(title);
        if (titleRes.IsFailure) return Result.Failure<Guid>(titleRes.Error);

        var subjectRes = SubjectAggregate.Create(titleRes.Value, semester);
        if (subjectRes.IsFailure) return Result.Failure<Guid>(subjectRes.Error);

        await subjectRepository.AddAsync(subjectRes.Value);
        return subjectRes.Value.Id;
    }

    public async Task<Result> UpdateSubjectAsync(Guid id, string title, int semester)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(id);
        if (subjectRes.IsFailure) return subjectRes;

        var titleRes = SubjectTitle.Create(title);
        if (titleRes.IsFailure) return titleRes;

        var updateRes = subjectRes.Value.Update(titleRes.Value, semester);
        if (updateRes.IsFailure) return updateRes;

        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }

    public async Task<Result> AddAssignmentAsync(Guid subjectId, string title, DateTime deadline)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(subjectId);
        if (subjectRes.IsFailure) return subjectRes;

        var titleRes = AssignmentTitle.Create(title);
        if (titleRes.IsFailure) return titleRes;

        var result = subjectRes.Value.AddAssignment(titleRes.Value, deadline);
        if (result.IsFailure) return result;

        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }

    public async Task<Result> UpdateAssignmentAsync(Guid subjectId, Guid assignmentId, string title, DateTime deadline)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(subjectId);
        if (subjectRes.IsFailure) return subjectRes;

        var titleRes = AssignmentTitle.Create(title);
        if (titleRes.IsFailure) return titleRes;

        var result = subjectRes.Value.UpdateAssignment(assignmentId, titleRes.Value, deadline);
        if (result.IsFailure) return result;

        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }

    public async Task<Result<List<StudentAggregate>>> GetDebtorsAsync(Guid assignmentId)
    {
        var subjectRes = await subjectRepository.GetByAssignmentIdAsync(assignmentId);
        if (subjectRes.IsFailure)
            return Result.Failure<List<StudentAggregate>>(subjectRes.Error);

        if (subjectRes.Value.TargetGroups.Count == 0)
            return new List<StudentAggregate>();

        var submittedIdsRes = await submissionRepository.GetStudentIdsByAssignmentAsync(assignmentId);
        if (submittedIdsRes.IsFailure)
            return Result.Failure<List<StudentAggregate>>(submittedIdsRes.Error);

        var groups = subjectRes.Value.TargetGroups.Select(g => g.GroupName.Value).ToList();
        var allStudentsRes = await studentRepository.GetByGroupsAsync(groups);
        if (allStudentsRes.IsFailure)
            return Result.Failure<List<StudentAggregate>>(allStudentsRes.Error);

        var submittedIds = submittedIdsRes.Value;
        var allStudentsInGroups = allStudentsRes.Value;

        return allStudentsInGroups
            .Where(s => !submittedIds.Contains(s.Id))
            .OrderBy(s => s.Group.Value)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public async Task<Result<SubjectAggregate>> GetSubjectByAssignmentIdAsync(Guid assignmentId)
    {
        return await subjectRepository.GetByAssignmentIdAsync(assignmentId);
    }

    public async Task<Result> DeleteSubjectAsync(Guid id) => await subjectRepository.DeleteAsync(id);

    public async Task<Result<List<SubjectAggregate>>> GetAllSubjectsAsync() => await subjectRepository.GetAllAsync();

    public async Task<Result<SubjectAggregate>> GetSubjectByIdAsync(Guid id) => await subjectRepository.GetByIdAsync(id);

    public async Task<Result> RemoveTargetGroupAsync(Guid subjectId, Guid targetGroupId)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(subjectId);
        if (subjectRes.IsFailure) return subjectRes;

        subjectRes.Value.RemoveTargetGroup(targetGroupId);
        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }

    public async Task<Result> AddTargetGroupAsync(Guid subjectId, string groupName)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(subjectId);
        if (subjectRes.IsFailure) return subjectRes;

        var groupRes = GroupName.Create(groupName);
        if (groupRes.IsFailure) return groupRes;

        subjectRes.Value.AddTargetGroup(groupRes.Value);
        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }

    public async Task<Result> DeleteAssignmentAsync(Guid subjectId, Guid assignmentId)
    {
        var subjectRes = await subjectRepository.GetByIdAsync(subjectId);
        if (subjectRes.IsFailure) return subjectRes;

        subjectRes.Value.RemoveAssignment(assignmentId);
        await subjectRepository.UpdateAsync(subjectRes.Value);
        return Result.Success();
    }
}