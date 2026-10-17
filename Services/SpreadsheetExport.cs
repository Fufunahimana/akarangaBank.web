using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace AkarangaBank.Web.Services;

public sealed record SpreadsheetTable(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<object?>> Rows);

public static class SpreadsheetExport
{
    public static byte[] BuildCsv(SpreadsheetTable table)
    {
        using var stream = new MemoryStream();
        using (var textWriter = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), 1024, leaveOpen: true))
        using (var csv = new CsvWriter(textWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false
        }))
        {
            foreach (var header in table.Headers)
                csv.WriteField(ProtectSpreadsheetFormula(header));
            csv.NextRecord();

            foreach (var row in table.Rows)
            {
                foreach (var value in row)
                    csv.WriteField(value is string text
                        ? ProtectSpreadsheetFormula(text)
                        : ToText(value));
                csv.NextRecord();
            }
        }

        return stream.ToArray();
    }

    public static byte[] BuildXlsx(SpreadsheetTable table)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Rapport");

        for (var column = 0; column < table.Headers.Count; column++)
            worksheet.Cell(1, column + 1).Value = table.Headers[column];

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];
            for (var column = 0; column < row.Count; column++)
                SetCellValue(worksheet.Cell(rowIndex + 2, column + 1), row[column]);
        }

        var lastColumn = Math.Max(table.Headers.Count, 1);
        var lastRow = Math.Max(table.Rows.Count + 1, 1);
        var header = worksheet.Range(1, 1, 1, lastColumn);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#102A43");
        header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        header.Style.Border.BottomBorderColor = XLColor.FromHtml("#C9A227");
        worksheet.Range(1, 1, lastRow, lastColumn).SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns(1, lastColumn).AdjustToContents();

        foreach (var column in worksheet.Columns(1, lastColumn))
        {
            if (column.Width > 55)
                column.Width = 55;
        }

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                return;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case int integerValue:
                cell.Value = integerValue;
                break;
            case long longValue:
                cell.Value = longValue;
                break;
            case DateTime dateTimeValue:
                cell.Value = dateTimeValue;
                cell.Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
                break;
            case DateOnly dateOnlyValue:
                cell.Value = dateOnlyValue.ToDateTime(TimeOnly.MinValue);
                cell.Style.DateFormat.Format = "dd/mm/yyyy";
                break;
            default:
                cell.Value = ToText(value);
                break;
        }
    }

    private static string ToText(object? value) => value switch
    {
        null => string.Empty,
        DateTime dateTimeValue => dateTimeValue.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
        DateOnly dateOnlyValue => dateOnlyValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string ProtectSpreadsheetFormula(string value) =>
        value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
            ? $"'{value}"
            : value;
}
