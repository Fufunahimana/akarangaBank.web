using System.Globalization;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

internal static class AmlCsvExport
{
    private static readonly string[] Headers =
    [
        "Id alerte", "Date (Africa/Bujumbura)", "Numéro client", "Nom du client", "Type", "Niveau", "Statut",
        "Référence opération", "Description"
    ];

    public static byte[] Build(IEnumerable<AlerteAML> alerts)
    {
        var table = new SpreadsheetTable(
            Headers,
            alerts.Select(alert => (IReadOnlyList<object?>)[
                alert.Id,
                BankTime.ToLocal(alert.DateAlerte).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                alert.Client?.NumeroClient,
                $"{alert.Client?.Nom} {alert.Client?.Prenom}".Trim(),
                alert.TypeAlerte,
                alert.Niveau,
                alert.Statut,
                alert.Operation?.Reference,
                alert.Description
            ]).ToList());
        return SpreadsheetExport.BuildCsv(table);
    }
}
