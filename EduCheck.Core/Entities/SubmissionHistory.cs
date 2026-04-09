namespace EduCheck.Core.Entities;

/// <summary>
/// Снимок (snapshot) конкретной загрузки файла студентом.
/// Хранит метаданные файла на момент получения.
/// </summary>
public class SubmissionHistory
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }

    /// <summary>
    /// Порядковый номер попытки.
    /// </summary>
    public int Version { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Путь к объекту во внешнем хранилище (локальный диск или S3/MinIO).
    /// </summary>
    public string FileStoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Контрольная сумма файла (SHA-256) для предотвращения дубликатов и базовой проверки на плагиат.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    /// <summary>
    /// Флаг, указывающий, была ли данная конкретная версия прислана позже дедлайна задания.
    /// </summary>
    public bool IsLate { get; set; }

    /// <summary>
    /// Результат автоматического анализа кода (в формате JSON или текстового отчета).
    /// </summary>
    public string? AnalysisResult { get; set; }
}