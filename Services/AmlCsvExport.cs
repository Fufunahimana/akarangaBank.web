using System.Globalization;
using System.Text;
using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

internal static class AmlCsvExport
{
    private static readonly string[] Headers =
    [
        "Id alerte", "Date (Africa/Bujumbura)", "Numero client", "Nom du client", "Type", "Niveau", "Statut",
        "Reference operation", "Description"
    ];

    public static byte[] Build(IEnumerable<AlerteAML> alerts)
    {
        var csv = new StringBuilder();
        csv.AppendLine(Row(Headers));

        foreach (var alert in alerts)
        {
            csv.AppendLine(Row(
                alert.Id.ToString(CultureInfo.InvariantCulture),
                BankTime.ToLocal(alert.DateAlerte).ToString("O", CultureInfo.InvariantCulture),
                alert.Client?.NumeroClient,
                $"{alert.Client?.Nom} {alert.Client?.Prenom}".Trim(),
                alert.TypeAlerte,
                alert.Niveau,
                alert.Statut,
                alert.Operation?.Reference,
                alert.Description));
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv.ToString());
    }

    public static string Row(params string?[] fields) => string.Join(",", fields.Select(Escape));

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            text = $"'{text}";

        return $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
