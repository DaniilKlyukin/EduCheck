namespace EduCheck.Core.Interfaces;

public interface IEmailService
{
    Task SendFeedbackAsync(string toEmail, string subject, string body);
}