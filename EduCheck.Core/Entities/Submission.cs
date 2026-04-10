using EduCheck.Core.Enums;

namespace EduCheck.Core.Entities;

/// <summary>
/// Представляет совокупность всех попыток сдачи конкретного задания студентом.
/// Является агрегирующим узлом для истории версий и рецензий.
/// </summary>
public class Submission
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    /// <summary>
    /// Текущий этап обработки работы (новое, в проверке, принято и т.д.).
    /// </summary>
    public SubmissionStatus Status { get; set; } = SubmissionStatus.New;

    /// <summary>
    /// Указывает, что хотя бы одна из версий была загружена после наступления дедлайна.
    /// </summary>
    public bool HasLateUpload { get; set; }

    /// <summary>
    /// Номер последней загруженной версии (инкрементируется при каждом новом файле).
    /// </summary>
    public int CurrentVersion { get; set; }

    /// <summary>
    /// Время последнего изменения (загрузка файла или выставление оценки).
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// Полная история загруженных файлов и их метаданных.
    /// </summary>
    public List<SubmissionHistory> History { get; set; } = new();
}