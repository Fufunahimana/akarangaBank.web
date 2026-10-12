using System.Globalization;
using System.Text;

namespace AkarangaBank.Web.Services;

/// <summary>
/// Produit un PDF texte simple sans dépendance externe.
/// </summary>
public static class AuditPdfBuilder
{
    private const int LinesPerPage = 56;
    private const int MaxLineLength = 105;

    public static byte[] Build(IEnumerable<string> reportLines)
    {
        var lines = reportLines.SelectMany(WrapLine).ToList();
        if (lines.Count == 0)
            lines.Add("Rapport d'audit");

        var pages = lines
            .Chunk(LinesPerPage)
            .Select(page => BuildPageContent(page))
            .ToList();

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(" ", Enumerable.Range(0, pages.Count).Select(i => $"{4 + i * 2} 0 R"))}] /Count {pages.Count} >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        for (var i = 0; i < pages.Count; i++)
        {
            var pageObject = 4 + i * 2;
            var contentObject = pageObject + 1;
            var contentBytes = Encoding.ASCII.GetBytes(pages[i]);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObject} 0 R >>");
            objects.Add($"<< /Length {contentBytes.Length} >>\nstream\n{pages[i]}endstream");
        }

        var document = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        var currentOffset = Encoding.ASCII.GetByteCount(document.ToString());
        for (var i = 0; i < objects.Count; i++)
        {
            var objectText = $"{i + 1} 0 obj\n{objects[i]}\nendobj\n";
            offsets.Add(currentOffset);
            document.Append(objectText);
            currentOffset += Encoding.ASCII.GetByteCount(objectText);
        }

        var xrefOffset = currentOffset;
        document.Append("xref\n0 ").Append(objects.Count + 1).Append("\n")
            .Append("0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
            document.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");

        document.Append("trailer\n<< /Size ").Append(objects.Count + 1)
            .Append(" /Root 1 0 R >>\nstartxref\n").Append(xrefOffset).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(document.ToString());
    }

    private static string BuildPageContent(IEnumerable<string> lines)
    {
        var content = new StringBuilder("BT\n/F1 9 Tf\n45 795 Td\n13 TL\n");
        foreach (var line in lines)
            content.Append('(').Append(EscapeForPdf(NormalizeForPdf(line))).Append(") Tj\nT*\n");
        content.Append("ET\n");
        return content.ToString();
    }

    private static IEnumerable<string> WrapLine(string value)
    {
        var normalized = NormalizeForPdf(value);
        if (normalized.Length == 0)
        {
            yield return string.Empty;
            yield break;
        }

        while (normalized.Length > MaxLineLength)
        {
            var splitAt = normalized.LastIndexOf(' ', MaxLineLength);
            if (splitAt <= 0)
                splitAt = MaxLineLength;
            yield return normalized[..splitAt];
            normalized = normalized[splitAt..].TrimStart();
        }

        yield return normalized;
    }

    private static string NormalizeForPdf(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (character <= 0x7f)
                result.Append(character);
            else if (character is '\u0153' or '\u0152')
                result.Append(character == '\u0153' ? "oe" : "OE");
            else if (character is '\u2013' or '\u2014' or '\u2212')
                result.Append('-');
            else if (character is '\u2018' or '\u2019')
                result.Append('\'');
            else if (character is '\u201c' or '\u201d')
                result.Append('"');
            else if (character == '\u2026')
                result.Append("...");
            else
                result.Append('?');
        }

        return result.ToString();
    }

    private static string EscapeForPdf(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
