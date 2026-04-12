namespace EduCheck.Application.Interfaces;

public interface IEmailService
{
    Task SendFeedbackAsync(string toEmail, string subject, string body);
}