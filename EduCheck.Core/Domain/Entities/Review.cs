using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Entities;

/// <summary>
/// Результат проверки работы преподавателем.
/// </summary>
public class Review : Entity
{
    public Guid SubmissionId { get; private set; }
    public SubmissionVersion SubmissionVersion { get; private set; }
    public Grade? Grade { get; private set; }
    public string? TeacherComment { get; private set; }
    public DateTime CheckedAt { get; private set; }

    private Review() { }

    internal Review(Guid submissionId, SubmissionVersion version, Grade? grade, string? comment)
        : base(Guid.NewGuid())
    {
        SubmissionId = submissionId;
        SubmissionVersion = version;
        Grade = grade;
        TeacherComment = comment;
        CheckedAt = DateTime.UtcNow;
    }
}