namespace AkarangaBank.Web.ViewModels;

public class TransactionCarteViewModel
{
    public int CarteId { get; set; }

    public string TypeTransaction { get; set; } = "Paiement";

    public decimal Montant { get; set; }
}
