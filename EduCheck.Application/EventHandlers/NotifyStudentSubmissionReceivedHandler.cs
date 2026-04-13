using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduCheck.Application.EventHandlers;

public class NotifyStudentSubmissionReceivedHandler(
    ISubmissionRepository submissionRepository,
    IStudentRepository studentRepository,
    IEmailService emailService,
    ILogger<NotifyStudentSubmissionReceivedHandler> logger) : INotificationHandler<SubmissionAttemptAddedEvent>
{
    public async Task Handle(SubmissionAttemptAddedEvent notification, CancellationToken cancellationToken)
    {
        var submissionRes = await submissionRepository.GetByIdAsync(notification.HistoryId, cancellationToken);

        if (submissionRes.IsFailure)
        {
            logger.LogError("Failed to retrieve submission with ID {notification.HistoryId}", notification.HistoryId);
            return;
        }

        var submission = submissionRes.Value;

        var studentRes = await studentRepository.GetByIdAsync(submission.StudentId, cancellationToken);

        if (studentRes.IsFailure)
        {
            logger.LogError("Failed to retrieve student with ID {submission.StudentId}", submission.StudentId);
            return;
        }

        var student = studentRes.Value;

        var subject = $"Работа получена: {submission.AssignmentId}";
        var body = $"Здравствуйте, {student.Name}!\n\n" +
                      $"Ваша попытка сдачи от {DateTime.Now:g} успешно получена системой и поставлена в очередь на проверку.";

        await emailService.SendFeedbackAsync(student.Email.Value, subject, body);
    }
}