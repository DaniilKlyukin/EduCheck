using EduCheck.Core.Domain.Enums;

namespace EduCheck.Application.DTOs;

public record SubmissionSummaryDto(
    Guid Id,
    string StudentName,
    string Group,
    string SubjectTitle,
    string AssignmentTitle,
    int Version,
    SubmissionStatus Status,
    bool HasLateUpload
);
