using LandEase.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace LandEase.Infrastructure.ExternalServices;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

   public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
{
    var settings = _config.GetSection("EmailSettings");

    var senderEmail = settings["SenderEmail"] ?? throw new Exception("SenderEmail not configured.");
    var senderName = settings["SenderName"] ?? "LandEase";

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(senderName, senderEmail));
    message.To.Add(new MailboxAddress(toName, toEmail));
    message.Subject = subject;
    message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(settings["Host"], int.Parse(settings["Port"]!), SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(senderEmail, settings["Password"]);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}
}