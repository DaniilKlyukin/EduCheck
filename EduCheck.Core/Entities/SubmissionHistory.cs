using EduCheck.Core.ValueObjects;

namespace EduCheck.Core.Entities;

/// <summary>
/// Снимок (snapshot) конкретной загрузки файла студентом.
/// Хранит метаданные файла на момент получения.
/// </summary>
public class SubmissionHistory
{
    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public int Version { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileStoragePath { get; private set; } = string.Empty;
    public FileHash FileHash { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public bool IsLate { get; private set; }
    public string? AnalysisResult { get; private set; }

    private SubmissionHistory() { } // Для EF

    internal SubmissionHistory(
        Guid submissionId,
        int version,
        string fileName,
        string storagePath,
        FileHash fileHash,
        bool isLate,
        string? analysisResult)
    {
        SubmissionId = submissionId;
        Version = version;
        FileName = fileName;
        FileStoragePath = storagePath;
        FileHash = fileHash ?? throw new ArgumentNullException(nameof(fileHash));
        IsLate = isLate;
        AnalysisResult = analysisResult;
        ReceivedAt = DateTime.UtcNow;
    }

    public void SetAnalysisResult(string result)
    {
        AnalysisResult = result;
    }
}