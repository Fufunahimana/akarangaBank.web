using System.Threading.Channels;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

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
        _notifications.Writer.WriteAsync(new EmailNotification(subject, htmlMessage), cancellationToken).AsTask();

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
        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Some existing accounts have their role in ApplicationUser.Role but do not
        // have a corresponding row in AspNetUserRoles. Include both representations.
        var activeUsers = await userManager.Users
            .Where(user => user.EstActif && user.Email != null)
            .Select(user => new { user.Email, user.Role })
            .ToListAsync(cancellationToken);
        foreach (var user in activeUsers)
        {
            if (!string.IsNullOrWhiteSpace(user.Email)
                && authorizedRoles.Contains(user.Role?.Trim() ?? string.Empty))
                recipients.Add(user.Email!);
        }

        foreach (var role in authorizedRoles)
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            foreach (var user in users
                .Where(user => user.EstActif && !string.IsNullOrWhiteSpace(user.Email))
                .Select(user => user.Email!))
            {
                recipients.Add(user);
            }
        }

        var distinctRecipients = recipients.ToList();
        if (distinctRecipients.Count == 0)
        {
            _logger.LogWarning(
                "Notification '{Subject}' non envoyée : aucun destinataire admin/gestionnaire actif avec e-mail.",
                notification.Subject);
            return;
        }

        foreach (var recipient in distinctRecipients)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await emailSender.SendEmailAsync(recipient, notification.Subject, notification.HtmlMessage);
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

    private sealed record EmailNotification(string Subject, string HtmlMessage);
}
