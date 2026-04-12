using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduCheck.Application.EventHandlers;

public class SubmissionReviewedHandler(
    IStudentRepository studentRepository,
    IEmailService emailService,
    ILogger<SubmissionReviewedHandler> logger) : INotificationHandler<SubmissionReviewedEvent>
{
    public async Task Handle(SubmissionReviewedEvent notification, CancellationToken ct)
    {
        var studentRes = await studentRepository.GetByIdAsync(notification.StudentId, ct);

        if (studentRes.IsFailure)
        {
            logger.LogError($"Не удалось отправить уведомление: студент {notification.StudentId}");
            return;
        }

        var subject = "Результат проверки работы";
        var body = $"Ваша работа была проверена. Новый статус: {notification.Status}. Зайдите в систему для просмотра деталей.";

        await emailService.SendFeedbackAsync(studentRes.Value.Email.Value, subject, body);

        logger.LogInformation($"Уведомление отправлено студенту {studentRes.Value.Email.Value}");
    }
}