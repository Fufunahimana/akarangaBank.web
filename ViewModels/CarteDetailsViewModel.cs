using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class CarteDetailsViewModel
{
    public Carte Carte { get; set; } = null!;
    public IReadOnlyList<TransactionCarte> Transactions { get; set; } = Array.Empty<TransactionCarte>();
}
