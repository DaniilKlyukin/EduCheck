using EduCheck.Core.Enums;

namespace EduCheck.Core.DTOs;

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
