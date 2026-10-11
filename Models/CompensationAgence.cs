namespace AkarangaBank.Web.Models;

public class CompensationAgence
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int AgenceId { get; set; }
    public Agence Agence { get; set; } = null!;

    // The existing ledger supports general-ledger codes without a customer account.
    // Keep this optional so compensation does not require a fictitious client account.
    public int? CompteCompensationId { get; set; }
    public Compte? CompteCompensation { get; set; }
    public string CodeCompteGeneral { get; set; } = string.Empty;

    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal Solde { get; set; }
    public DateTime DateCompensation { get; set; } = DateTime.UtcNow;
    public DateTime? DateReglement { get; set; }
    public string Statut { get; set; } = "EnCours";
}
