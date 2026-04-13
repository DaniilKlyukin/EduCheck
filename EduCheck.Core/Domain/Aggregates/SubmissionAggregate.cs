using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.Enums;
using EduCheck.Core.Domain.Events;
using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Aggregates;

/// <summary>
/// Представляет совокупность всех попыток сдачи конкретного задания студентом.
/// Является агрегирующим узлом для истории версий и рецензий.
/// </summary>
public class SubmissionAggregate : AggregateRoot
{
    public Guid StudentId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public SubmissionVersion? CurrentVersion { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    private readonly List<SubmissionHistory> _history = new();
    public IReadOnlyCollection<SubmissionHistory> History => _history.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private SubmissionAggregate() { }

    public static SubmissionAggregate Create(Guid studentId, Guid assignmentId)
    {
        var submission = new SubmissionAggregate
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            AssignmentId = assignmentId,
            Status = SubmissionStatus.PendingAnalysis,
            CurrentVersion = null,
            LastActivityAt = DateTime.UtcNow
        };
        return submission;
    }

    public Result AddAttempt(FileMetadata file, DateTime deadline)
    {
        if (Status == SubmissionStatus.Accepted)
            return Result.Failure("Submission.Locked", "Нельзя загрузить новую версию в принятую работу.");

        if (_history.Any(h => h.FileHash == file.Hash))
            return Result.Failure("Submission.Duplicate", "Этот файл уже был загружен ранее.");

        CurrentVersion = CurrentVersion == null
            ? SubmissionVersion.Initial
            : CurrentVersion.Increment();

        var isLate = DateTime.UtcNow > deadline;

        var historyEntry = new SubmissionHistory(Id, CurrentVersion, file, isLate);
        _history.Add(historyEntry);

        Status = SubmissionStatus.PendingAnalysis;
        LastActivityAt = DateTime.UtcNow;

        RaiseDomainEvent(new SubmissionAttemptAddedEvent(Id, historyEntry.Id, CurrentVersion.Value));
        return Result.Success();
    }

    public Result CompleteAnalysis(Guid historyId, string finalReport)
    {
        var entry = _history.FirstOrDefault(h => h.Id == historyId);
        if (entry == null) return Result.Failure("History.NotFound", "Запись истории не найдена.");

        var updateResult = entry.UpdateAnalysisResult(finalReport);
        if (updateResult.IsFailure) return updateResult;

        if (Status == SubmissionStatus.PendingAnalysis)
        {
            Status = SubmissionStatus.New;
        }

        LastActivityAt = DateTime.UtcNow;

        RaiseDomainEvent(new SubmissionAnalysisCompletedEvent(Id, StudentId, Status.ToString()));

        return Result.Success();
    }

    public Result Review(Grade? grade, string? comment, SubmissionStatus nextStatus)
    {
        if (!IsValidTransition(nextStatus))
            return Result.Failure("Submission.InvalidState", $"Нельзя перевести работу из {Status} в {nextStatus}");

        Status = nextStatus;
        LastActivityAt = DateTime.UtcNow;

        var review = new Review(Id, CurrentVersion, grade, comment);
        _reviews.Add(review);

        RaiseDomainEvent(new SubmissionReviewedEvent(Id, StudentId, Status.ToString()));
        return Result.Success();
    }

    private bool IsValidTransition(SubmissionStatus next) =>
        (Status, next) switch
        {
            (SubmissionStatus.New, SubmissionStatus.InReview) => true,
            (SubmissionStatus.InReview, SubmissionStatus.UpdateRequired) => true,
            (SubmissionStatus.InReview, SubmissionStatus.Accepted) => true,
            (SubmissionStatus.UpdateRequired, SubmissionStatus.PendingAnalysis) => true,
            _ => false
        };
}