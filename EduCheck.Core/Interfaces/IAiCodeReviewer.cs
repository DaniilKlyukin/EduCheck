namespace EduCheck.Core.Interfaces;

public interface IAiCodeReviewer
{
    Task<string> GetReviewAsync(string allCode, CancellationToken ct = default);
}