using EduCheck.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EduCheck.Infrastructure.Services;

public class SmtpEmailService(IConfiguration config) : IEmailService
{
    public async Task SendFeedbackAsync(string toEmail, string subject, string body)
    {
        var settings = config.GetSection("EmailSettings");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("EduCheck Bot", settings["Email"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.mail.ru", 465, true);
        await client.AuthenticateAsync(settings["Email"], settings["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}