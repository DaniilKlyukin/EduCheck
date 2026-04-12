using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Events;

public record SubmissionAttemptAddedEvent(Guid SubmissionId, Guid HistoryId, int Version) : IDomainEvent;
public record SubmissionReviewedEvent(Guid SubmissionId, Guid StudentId, string Status) : IDomainEvent;
public record AssignmentCreatedEvent(Guid SubjectId, Guid AssignmentId) : IDomainEvent;
public record SubjectCreatedEvent(Guid SubjectId, string Title) : IDomainEvent;
public record SubmissionAnalysisCompletedEvent(Guid SubmissionId, Guid StudentId, string Status) : IDomainEvent;
