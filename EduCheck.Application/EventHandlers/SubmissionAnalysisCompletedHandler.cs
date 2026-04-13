using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EduCheck.Application.EventHandlers;

public class SubmissionAnalysisCompletedHandler(
    IStudentRepository studentRepository,
    IEmailService emailService,
    ILogger<SubmissionAnalysisCompletedHandler> logger) : INotificationHandler<SubmissionAnalysisCompletedEvent>
{
    public async Task Handle(SubmissionAnalysisCompletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation($"Обработка завершения анализа для работы {notification.SubmissionId}");

        var studentRes = await studentRepository.GetByIdAsync(notification.StudentId, ct);
        if (studentRes.IsFailure)
        {
            logger.LogError($"Не удалось отправить уведомление об анализе: студент {notification.StudentId} не найден");
            return;
        }

        var subject = "Автоматический анализ завершен";
        var body = $"""
            Здравствуйте, {studentRes.Value.Name}!
        
            Автоматический анализ вашей работы успешно завершен.
            Текущий статус работы: {notification.Status}.

            Работа ожидает ручной проверки преподавателем.
            """;

        try
        {
            await emailService.SendFeedbackAsync(studentRes.Value.Email.Value, subject, body);
            logger.LogInformation($"Уведомление об анализе отправлено на {studentRes.Value.Email.Value}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Ошибка при физической отправке Email для студента {notification.StudentId}");
        }
    }
}