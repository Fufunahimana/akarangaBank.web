using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace AkarangaBank.Web.Services;

public sealed class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = _configuration["Email:Smtp:Host"];
        var fromAddress = _configuration["Email:FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
            throw new InvalidOperationException(
                "L'envoi d'e-mails nécessite Email:Smtp:Host et Email:FromAddress.");

        var port = _configuration.GetValue("Email:Smtp:Port", 587);
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException(
                "Email:Smtp:Username est obligatoire pour l'authentification SMTP.");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Email:Smtp:Password est obligatoire pour l'authentification SMTP.");

        var secureSocketOptions = ResolveSecureSocketOptions(
            _configuration["Email:Smtp:SecureSocketOptions"]);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:FromName"] ?? "AkarangaBank",
            fromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(host, port, secureSocketOptions);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }

    private static SecureSocketOptions ResolveSecureSocketOptions(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? SecureSocketOptions.StartTls
            : Enum.TryParse<SecureSocketOptions>(value, ignoreCase: true, out var options)
                ? options
                : throw new InvalidOperationException(
                    $"Email:Smtp:SecureSocketOptions invalide : '{value}'.");
}
