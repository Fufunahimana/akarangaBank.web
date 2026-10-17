using System.Threading.Channels;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AkarangaBank.Web.Services;

public sealed class AdminNotificationQueue : BackgroundService, IAdminNotificationService
{
    private readonly Channel<EmailNotification> _notifications =
        Channel.CreateBounded<EmailNotification>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdminNotificationQueue> _logger;

    public AdminNotificationQueue(
        IServiceScopeFactory scopeFactory,
        ILogger<AdminNotificationQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task NotifyAdminsAndManagersAsync(
        string subject,
        string htmlMessage,
        CancellationToken cancellationToken = default) =>
        EnqueueAsync(new EmailNotification(subject, htmlMessage, null, null), cancellationToken);

    public Task NotifyAdminsAndManagersAsync(
        string subject,
        string htmlMessage,
        string actionUrl,
        string actionLabel,
        CancellationToken cancellationToken = default) =>
        EnqueueAsync(new EmailNotification(subject, htmlMessage, actionUrl, actionLabel), cancellationToken);

    private async Task EnqueueAsync(EmailNotification notification, CancellationToken cancellationToken)
    {
        await _notifications.Writer.WriteAsync(notification, cancellationToken);
        _logger.LogInformation("Notification e-mail '{Subject}' ajoutée à la file d'envoi.", notification.Subject);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in _notifications.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SendToAuthorizedRecipientsAsync(notification, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Impossible d'envoyer la notification '{Subject}' aux rôles admin/gestionnaire.",
                    notification.Subject);
            }
        }
    }

    private async Task SendToAuthorizedRecipientsAsync(
        EmailNotification notification,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var authorizedRoles = new HashSet<string>(new[] { "admin", "gestionnaire" }, StringComparer.OrdinalIgnoreCase);
        var recipients = new Dictionary<string, NotificationRecipient>(StringComparer.OrdinalIgnoreCase);

        // Some existing accounts have their role in ApplicationUser.Role but do not
        // have a corresponding row in AspNetUserRoles. Include both representations.
        var activeUsers = await userManager.Users
            .Where(user => user.EstActif && user.Email != null)
            .Select(user => new { user.Email, user.NomComplet, user.Role })
            .ToListAsync(cancellationToken);
        foreach (var user in activeUsers)
        {
            if (!string.IsNullOrWhiteSpace(user.Email)
                && authorizedRoles.Contains(user.Role?.Trim() ?? string.Empty))
                recipients.TryAdd(user.Email!, new NotificationRecipient(user.Email!, user.NomComplet, user.Role!));
        }

        foreach (var role in authorizedRoles)
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            foreach (var user in users.Where(user => user.EstActif && !string.IsNullOrWhiteSpace(user.Email)))
            {
                var email = user.Email!;
                recipients.TryAdd(email, new NotificationRecipient(email, user.NomComplet, role));
            }
        }

        if (recipients.Count == 0)
        {
            _logger.LogWarning(
                "Notification '{Subject}' non envoyée : aucun destinataire admin/gestionnaire actif avec e-mail.",
                notification.Subject);
            return;
        }

        _logger.LogInformation(
            "Envoi de la notification '{Subject}' à {RecipientCount} destinataire(s) admin/gestionnaire.",
            notification.Subject,
            recipients.Count);

        foreach (var recipient in recipients.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var htmlMessage = BuildPersonalizedMessage(notification, recipient);
                await emailSender.SendEmailAsync(recipient.Email, notification.Subject, htmlMessage);
                _logger.LogInformation(
                    "Notification '{Subject}' envoyée à un destinataire admin/gestionnaire.",
                    notification.Subject);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Échec de l'envoi de la notification '{Subject}' à un destinataire autorisé.",
                    notification.Subject);
            }
        }
    }

    private static string BuildPersonalizedMessage(EmailNotification notification, NotificationRecipient recipient)
    {
        var name = string.IsNullOrWhiteSpace(recipient.FullName) ? recipient.Email : recipient.FullName.Trim();
        var role = recipient.Role.Trim().ToLowerInvariant() switch
        {
            "admin" => "admin",
            "gestionnaire" => "gestionnaire",
            _ => recipient.Role.Trim()
        };
        var localHour = BankTime.Now.Hour;
        var greeting = localHour >= 18 || localHour < 6 ? "Bonsoir" : "Bonjour";
        var actionLink = notification.ActionUrl is null
            ? string.Empty
            : $"<p><a href=\"{WebUtility.HtmlEncode(notification.ActionUrl)}\" " +
              "style=\"display:inline-block;padding:12px 18px;background:#17365d;color:#ffffff;text-decoration:none;border-radius:4px;\">" +
              $"{WebUtility.HtmlEncode(notification.ActionLabel ?? "Ouvrir")}</a></p>";

        return $"<p>{greeting} {WebUtility.HtmlEncode(name)} ({WebUtility.HtmlEncode(role)} chez Akaranga Bank),</p>" +
               notification.HtmlMessage +
               actionLink;
    }

    private sealed record EmailNotification(string Subject, string HtmlMessage, string? ActionUrl, string? ActionLabel);
    private sealed record NotificationRecipient(string Email, string? FullName, string Role);
}
