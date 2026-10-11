namespace AkarangaBank.Web.Models; 
 
public class AuditLog 
{ 
    public int Id { get; set; } 
    public DateTime DateEvenement { get; set; } = DateTime.UtcNow; 
    public int? UtilisateurId { get; set; } 
    public ApplicationUser? Utilisateur { get; set; }
    public int? AgenceId { get; set; } 
    public Agence? Agence { get; set; } 
    public string Action { get; set; } = string.Empty; 
    public string? TableConcernee { get; set; } 
    public int? EnregistrementId { get; set; } 
    public string? ValeurAvant { get; set; } 
    public string? ValeurApres { get; set; } 
    public string? AdresseIP { get; set; } 
    public string? UserAgent { get; set; } 
    public string? Details { get; set; } 
}
