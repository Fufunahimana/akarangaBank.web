namespace AkarangaBank.Web.Services;

public interface IAdminNotificationService
{
    Task NotifyAdminsAndManagersAsync(string subject, string htmlMessage, CancellationToken cancellationToken = default);
}
