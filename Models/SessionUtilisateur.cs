namespace AkarangaBank.Web.Models; 
 
public class SessionUtilisateur 
{ 
    public int Id { get; set; } 
    public int UtilisateurId { get; set; } 
    public ApplicationUser Utilisateur { get; set; } = null!;
    public string Token { get; set; } = string.Empty; 
    public string? AdresseIP { get; set; } 
    public string? UserAgent { get; set; } 
    public bool DeuxFA_Valide { get; set; } 
    public DateTime DateConnexion { get; set; } = DateTime.UtcNow; 
    public DateTime DateExpiration { get; set; } 
    public bool EstActive { get; set; } = true; 
}
