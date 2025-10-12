using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
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
        // ✅ Leer configuración con validación
        var fromName = _config["Email:FromName"] ?? throw new InvalidOperationException("Email:FromName no configurado.");
        var fromAddress = _config["Email:FromAddress"] ?? throw new InvalidOperationException("Email:FromAddress no configurado.");
        var smtpHost = _config["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost no configurado.");

        var portString = _config["Email:SmtpPort"] ?? throw new InvalidOperationException("Email:SmtpPort no configurado.");
        if (!int.TryParse(portString, out var smtpPort))
            throw new InvalidOperationException("Email:SmtpPort inválido.");

        // ✨ Usaremos STARTTLS si así lo configuramos
        var secureOption = SecureSocketOptions.StartTls;

        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        // 📧 Crear el mensaje
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = htmlBody
        };

        using var client = new SmtpClient();

        // 📡 Conexión segura con STARTTLS (recomendada para Gmail en puerto 587)
        await client.ConnectAsync(smtpHost, smtpPort, secureOption);

        // 🧑‍💻 Autenticación
        if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
        {
            await client.AuthenticateAsync(smtpUser, smtpPass);
        }

        // 📤 Enviar y cerrar
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
