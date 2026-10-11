using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class JournalViewModel
{
    public IEnumerable<Operation> Operations { get; set; } = new List<Operation>();
    public DateTime? Du { get; set; }
    public DateTime? Au { get; set; }
    public string? TypeOperation { get; set; }
    public int Page { get; set; } = 1;
    public int TaillePage { get; set; } = 20;
    public int Total { get; set; }
    public int TotalPages => TaillePage <= 0 ? 0 : (int)Math.Ceiling((double)Total / TaillePage);
}
