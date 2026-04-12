using EduCheck.Core.Domain.ValueObjects;
using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Entities;

/// <summary>
/// Снимок (snapshot) конкретной загрузки файла студентом.
/// Хранит метаданные файла на момент получения.
/// </summary>
public class SubmissionHistory : Entity
{
    public Guid SubmissionId { get; private set; }
    public int Version { get; private set; }
    public FileMetadata File { get; private set; }
    public FileHash FileHash => File.Hash;
    public DateTime ReceivedAt { get; private set; }
    public bool IsLate { get; private set; }
    public string? AnalysisResult { get; private set; }

    private SubmissionHistory() { }

    internal SubmissionHistory(Guid submissionId, int version, FileMetadata file, bool isLate)
        : base(Guid.NewGuid())
    {
        SubmissionId = submissionId;
        Version = version;
        File = file;
        IsLate = isLate;
        ReceivedAt = DateTime.UtcNow;
    }

    internal Result UpdateAnalysisResult(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
            return Result.Failure("AnalysisResult.Empty", "Result cannot be empty");

        AnalysisResult = result;
        return Result.Success();
    }
}