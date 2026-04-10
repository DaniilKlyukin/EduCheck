using EduCheck.Core.DTOs;
using EduCheck.Core.Entities;
using EduCheck.Core.Enums;

namespace EduCheck.Core.Interfaces;

public interface ISubmissionService
{
    Task<List<SubmissionSummaryDto>> GetAllSubmissionsAsync();
    Task<Submission?> GetSubmissionByIdAsync(Guid id);
    Task SubmitReviewAsync(Guid submissionId, Review review, SubmissionStatus newStatus);
}
