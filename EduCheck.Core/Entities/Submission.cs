using EduCheck.Core.Enums;
using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Представляет совокупность всех попыток сдачи конкретного задания студентом.
/// Является агрегирующим узлом для истории версий и рецензий.
/// </summary>
public class Submission
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public bool HasLateUpload { get; private set; }
    public int CurrentVersion { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    private readonly List<SubmissionHistory> _history = new();
    public IReadOnlyCollection<SubmissionHistory> History => _history.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    public Student Student { get; private set; } = null!;
    public Assignment Assignment { get; private set; } = null!;

    private Submission() { } // Для EF

    public Submission(Guid studentId, Guid assignmentId)
    {
        Id = Guid.NewGuid();
        StudentId = studentId;
        AssignmentId = assignmentId;
        Status = SubmissionStatus.PendingAnalysis;
        CurrentVersion = 0;
        HasLateUpload = false;
        LastActivityAt = DateTime.UtcNow;
    }

    public void AddAttempt(string fileName, string storagePath, FileHash fileHash, string analysisResult, DateTime assignmentDeadline)
    {
        if (_history.Any(h => h.FileHash == fileHash))
            return;

        CurrentVersion++;
        LastActivityAt = DateTime.UtcNow;

        bool isLate = DateTime.UtcNow > assignmentDeadline;
        if (isLate) HasLateUpload = true;

        if (Status == SubmissionStatus.UpdateRequired || Status == SubmissionStatus.Accepted)
            Status = SubmissionStatus.PendingAnalysis;

        var history = new SubmissionHistory(Id, CurrentVersion, fileName, storagePath, fileHash, isLate, analysisResult);
        _history.Add(history);
    }

    public void ReviewSubmission(Grade? grade, string? comment, SubmissionStatus newStatus)
    {
        Status = newStatus;
        LastActivityAt = DateTime.UtcNow;

        var review = new Review(Id, CurrentVersion, grade, comment);
        _reviews.Add(review);
    }

    public void CompleteAnalysis(string finalReport)
    {
        if (Status == SubmissionStatus.PendingAnalysis)
        {
            Status = SubmissionStatus.New;
        }

        LastActivityAt = DateTime.UtcNow;
    }
}