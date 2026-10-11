namespace AkarangaBank.Web.Models; 
 
public class Virement 
{ 
    public int Id { get; set; } 
    public string Reference { get; set; } = string.Empty; 
    public int CompteSourceId { get; set; } 
    public Compte CompteSource { get; set; } = null!; 
    public int CompteDestinationId { get; set; } 
    public Compte CompteDestination { get; set; } = null!; 
    public int UtilisateurId { get; set; } 
    public ApplicationUser Utilisateur { get; set; } = null!; 
    public decimal Montant { get; set; } 
    public string Devise { get; set; } = "BIF"; 
    public string Statut { get; set; } = "EnAttente"; 
    public string? Motif { get; set; } 
    public bool ControleAML { get; set; } 
    public DateTime DateVirement { get; set; } = DateTime.UtcNow; 
    public DateTime? DateValidation { get; set; } 
} 
