namespace AkarangaBank.Web.Models; 
 
public class AlerteAML 
{ 
    public int Id { get; set; } 
    public int ClientId { get; set; } 
    public Client Client { get; set; } = null!; 
    public int? OperationId { get; set; } 
    public Operation? Operation { get; set; } 
    public string TypeAlerte { get; set; } = string.Empty; 
    public string Niveau { get; set; } = "Moyen";   // Faible, Moyen, Eleve, Critique 
    public string? Description { get; set; } 
    public string Statut { get; set; } = "Ouverte"; // Ouverte, EnCours, Traitee, Classee 
    public int? TraiteParId { get; set; } 
    public ApplicationUser? TraitePar { get; set; } 
    public DateTime DateAlerte { get; set; } = DateTime.UtcNow; 
    public DateTime? DateTraitement { get; set; } 
} 
