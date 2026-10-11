namespace AkarangaBank.Web.Models;

public class VirementInterAgence
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;

    public int VirementId { get; set; }
    public Virement Virement { get; set; } = null!;

    public int AgenceSourceId { get; set; }
    public Agence AgenceSource { get; set; } = null!;

    public int AgenceDestinationId { get; set; }
    public Agence AgenceDestination { get; set; } = null!;

    public decimal Montant { get; set; }
    public string Devise { get; set; } = "BIF";
    public string Statut { get; set; } = "EnAttente";
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateCompensation { get; set; }
    public DateTime? DateReglement { get; set; }
    public string? MotifRejet { get; set; }
    public int? ValideParId { get; set; }
    public ApplicationUser? ValidePar { get; set; }
}
