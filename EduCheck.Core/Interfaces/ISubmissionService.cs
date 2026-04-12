using EduCheck.Core.DTOs;
using EduCheck.Core.Entities;
using EduCheck.Core.Enums;

namespace EduCheck.Core.Interfaces;

public interface ISubmissionService
{
    Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync();
    Task<Submission?> GetSubmissionByIdAsync(Guid id);

    Task SubmitReviewAsync(Guid submissionId, int? grade, string? comment, SubmissionStatus newStatus);

    Task<string> GetDownloadUrlAsync(Guid historyId);
    Task DeleteSubmissionAsync(Guid id);
}