using EduCheck.Core.Enums;
using EduCheck.Core.Interfaces;

namespace EduCheck.Infrastructure.Services;

public class SubmissionStatusLabelProvider : ISubmissionStatusLabelProvider
{
    public string GetDisplayName(SubmissionStatus status) => status switch
    {
        SubmissionStatus.New => "Новое",
        SubmissionStatus.InReview => "На проверке",
        SubmissionStatus.Accepted => "Принято",
        SubmissionStatus.UpdateRequired => "Требует доработки",
        _ => status.ToString()
    };
}
