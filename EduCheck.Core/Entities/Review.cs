using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Результат проверки работы преподавателем.
/// </summary>
public class Review
{
    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public int SubmissionVersion { get; private set; }
    public Grade? Grade { get; private set; }
    public string? TeacherComment { get; private set; }
    public DateTime CheckedAt { get; private set; }

    private Review() { } // Для EF

    internal Review(Guid submissionId, int version, Grade? grade, string? comment)
    {
        SubmissionId = submissionId;
        SubmissionVersion = version;
        Grade = grade;
        TeacherComment = comment;
        CheckedAt = DateTime.UtcNow;
    }
}