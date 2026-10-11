namespace AkarangaBank.Web.Models; 
 
public class Pret{ 
    public int Id { get; set; } 
    public string Reference { get; set; } = string.Empty; 
    public int ClientId { get; set; } 
    public Client Client { get; set; } = null!; 
    public int TypePretId { get; set; }
    public TypePret TypePret { get; set; } = null!;
    public int CompteDebitId { get; set; } 
    public Compte CompteDebit { get; set; } = null!; 
    public int? ValideParId { get; set; } 
    public ApplicationUser? ValidePar { get; set; }
    public decimal MontantPrincipal { get; set; } 
    public decimal TauxInteret { get; set; } 
    public int DureeMois { get; set; } 
    public decimal Mensualite { get; set; } 
    public decimal MontantRembourse { get; set; }
    public decimal SoldeRestant { get; set; }
    public string Statut { get; set; } = "EnAttente"; 
    public string? Commentaire { get; set; }
    public string? MotifRejet { get; set; }
    public DateTime DateDemande { get; set; } = DateTime.UtcNow; 
    public DateTime? DateValidation { get; set; } 
    public DateTime? DateDebut { get; set; } 
    public DateTime? DateFin { get; set; } 
 
    // Navigation  
    public ICollection<EcheancePret> Echeances { get; set; } = new List<EcheancePret>(); 
} 
