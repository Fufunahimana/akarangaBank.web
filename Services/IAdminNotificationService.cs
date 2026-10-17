namespace AkarangaBank.Web.Services;

public interface IAdminNotificationService
{
    Task NotifyAdminsAndManagersAsync(string subject, string htmlMessage, CancellationToken cancellationToken = default);

    Task NotifyAdminsAndManagersAsync(
        string subject,
        string htmlMessage,
        string actionUrl,
        string actionLabel,
        CancellationToken cancellationToken = default);
}
