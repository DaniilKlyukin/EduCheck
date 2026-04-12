using EduCheck.Application.DTOs;
using EduCheck.Core.Domain.Aggregates;
using EduCheck.Core.Domain.Enums;
using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface ISubmissionService
{
    Task<Result<List<SubmissionSummaryDto>>> GetAllSubmissionsAsync();
    Task<Result<SubmissionAggregate>> GetSubmissionByIdAsync(Guid id);

    Task<Result> SubmitReviewAsync(Guid submissionId, int? grade, string? comment, SubmissionStatus newStatus);

    Task<Result<string>> GetDownloadUrlAsync(Guid historyId);
    Task<Result> DeleteSubmissionAsync(Guid id);
}