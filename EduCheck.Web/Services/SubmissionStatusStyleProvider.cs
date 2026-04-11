using EduCheck.Core.Enums;
using EduCheck.Web.Interfaces;

namespace EduCheck.Web.Services;

public class SubmissionStatusStyleProvider : ISubmissionStatusStyleProvider
{
    public string GetBadgeClass(SubmissionStatus status) => status switch
    {
        SubmissionStatus.New => "bg-primary",
        SubmissionStatus.InReview => "bg-info text-dark",
        SubmissionStatus.Accepted => "bg-success",
        SubmissionStatus.UpdateRequired => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}
