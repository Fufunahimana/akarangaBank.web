using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.ViewModels;

public sealed record ReportTableViewModel(
    string Title,
    string ActionName,
    SpreadsheetTable Table,
    Dictionary<string, string> RouteValues);
