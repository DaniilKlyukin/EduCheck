using EduCheck.Application.DTOs;
using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Enums;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Services;

public class SubmissionService(
    ISubmissionRepository submissionRepository,
    IFileStorage fileStorage) : ISubmissionService
{
    public async Task<Result<List<SubmissionSummaryDto>>> GetAllSubmissionsAsync()
    {
        return await submissionRepository.GetAllSummariesAsync();
    }

    public async Task<Result<SubmissionAggregate>> GetSubmissionByIdAsync(Guid id)
    {
        return await submissionRepository.GetByIdAsync(id);
    }

    public async Task<Result> SubmitReviewAsync(Guid submissionId, int? gradeValue, string? comment, SubmissionStatus newStatus)
    {
        var submissionRes = await submissionRepository.GetByIdAsync(submissionId);
        if (submissionRes.IsFailure) return submissionRes.Error;

        var submission = submissionRes.Value;

        Grade? grade = null;
        if (gradeValue.HasValue)
        {
            var gradeRes = Grade.Create(gradeValue.Value);
            if (gradeRes.IsFailure) return gradeRes.Error;
            grade = gradeRes.Value;
        }

        var reviewResult = submission.Review(grade, comment, newStatus);
        if (reviewResult.IsFailure) return reviewResult.Error;

        return await submissionRepository.UpdateAsync(submission);
    }

    public async Task<Result<string>> GetDownloadUrlAsync(Guid historyId)
    {
        var historyRes = await submissionRepository.GetHistoryByIdAsync(historyId);
        if (historyRes.IsFailure) return historyRes.Error;

        var history = historyRes.Value;

        return await fileStorage.GetDownloadUrlAsync(history.File.StoragePath, history.File.Name);
    }

    public async Task<Result> DeleteSubmissionAsync(Guid id)
    {
        return await submissionRepository.DeleteAsync(id);
    }
}