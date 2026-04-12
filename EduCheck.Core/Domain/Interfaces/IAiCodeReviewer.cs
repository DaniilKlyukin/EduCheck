using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Interfaces;

public interface IAiCodeReviewer
{
    Task<Result<string>> GetReviewAsync(string allCode, CancellationToken ct = default);
}