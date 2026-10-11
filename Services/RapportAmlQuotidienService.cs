using System.Globalization;
using System.Text;
using AkarangaBank.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public sealed class RapportAmlQuotidienService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RapportAmlQuotidienService> _logger;
    private readonly TimeZoneInfo _timeZone;

    public RapportAmlQuotidienService(
        IServiceScopeFactory scopeFactory,
        IWebHostEnvironment environment,
        ILogger<RapportAmlQuotidienService> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
        _timeZone = BankTime.TimeZone;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZone);
            var nextRun = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 5, 0, now.Offset);
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            try
            {
                await Task.Delay(nextRun - now, stoppingToken);
                await GenererRapportAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Échec de génération du rapport AML quotidien.");
            }
        }
    }

    private async Task GenererRapportAsync(CancellationToken cancellationToken)
    {
        var maintenantLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZone);
        var dateRapport = DateOnly.FromDateTime(maintenantLocal.Date.AddDays(-1));
        await GenererRapportPourDateAsync(dateRapport, cancellationToken);
    }

    public async Task<string> GenererRapportPourDateAsync(DateOnly dateRapport, CancellationToken cancellationToken = default)
    {
        var debutLocale = DateTime.SpecifyKind(dateRapport.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var finLocale = debutLocale.AddDays(1);
        var debutUtc = TimeZoneInfo.ConvertTimeToUtc(debutLocale, _timeZone);
        var finUtc = TimeZoneInfo.ConvertTimeToUtc(finLocale, _timeZone);
        var jourRapport = dateRapport.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AkarangaBankContext>();
        var alerts = await context.AlertesAML.AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Operation)
            .Where(a => a.DateAlerte >= debutUtc && a.DateAlerte < finUtc)
            .OrderBy(a => a.DateAlerte)
            .ToListAsync(cancellationToken);

        var reportDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "RapportsAML");
        Directory.CreateDirectory(reportDirectory);
        var filePath = Path.Combine(reportDirectory, $"rapport-aml-{jourRapport}.csv");
        await File.WriteAllBytesAsync(filePath, AmlCsvExport.Build(alerts), cancellationToken);
        _logger.LogInformation("Rapport AML du {ReportDate} écrit dans {ReportPath} ({AlertCount} alertes).",
            jourRapport, filePath, alerts.Count);
        return filePath;
    }

}
