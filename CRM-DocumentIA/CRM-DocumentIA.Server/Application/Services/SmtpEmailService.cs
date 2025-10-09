using System.Net.Mail;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

public class SmtpEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["Email:FromName"], _config["Email:FromAddress"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = htmlBody
        };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(
            _config["Email:SmtpHost"],
            int.Parse(_config["Email:SmtpPort"]),
            bool.Parse(_config["Email:SmtpUseSsl"])
        );

        if (!string.IsNullOrEmpty(_config["Email:SmtpUser"]))
        {
            await client.AuthenticateAsync(
                _config["Email:SmtpUser"],
                _config["Email:SmtpPass"]
            );
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}