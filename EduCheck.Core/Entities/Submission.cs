using EduCheck.Core.Enums;

namespace EduCheck.Core.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string SubjectTitle {  get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileStoragePath { get; set; } = string.Empty;
    public string FileHash { get; set;  } = string.Empty;
    public int Version { get; set; } = 1;
    public DateTime ReceiveAt { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
    public string? TeacherComment { get; set; }
    public int? Grade { get; set; }
}