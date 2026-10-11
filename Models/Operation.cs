namespace AkarangaBank.Web.Models; 
 
public class Operation 
{ 
    public int Id { get; set; } 
    public string Reference { get; set; } = string.Empty; 
    public string TypeOperation { get; set; } = string.Empty; 
    public int CompteId { get; set; } 
    public Compte Compte { get; set; } = null!; 
    public int UtilisateurId { get; set; } 
    public ApplicationUser Utilisateur { get; set; } = null!; 
    public int AgenceId { get; set; } 
    public Agence Agence { get; set; } = null!; 
    public int? VirementId { get; set; } 
    public Virement? Virement { get; set; } 
    public int? PretId { get; set; } 
    public Pret? Pret { get; set; } 
    public decimal Montant { get; set; } 
    public string Devise { get; set; } = "BIF"; 
    public string Statut { get; set; } = "EnAttente"; 
    public DateTime DateOperation { get; set; } = DateTime.UtcNow; 
    public DateTime? DateValidation { get; set; } 
    public string? Commentaire { get; set; } 
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
 
    // Navigation 
    public ICollection<Ecriture> Ecritures { get; set; } = new 
List<Ecriture>(); 
} 
