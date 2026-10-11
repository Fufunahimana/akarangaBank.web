namespace AkarangaBank.Web.Models; 
 
public class Utilisateur 
{ 
    public int Id { get; set; } 
    public string NomComplet { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty; 
    public string MotDePasseHash { get; set; } = string.Empty; 
    public string Role { get; set; } = string.Empty; 
    public int AgenceId { get; set; } 
    public Agence Agence { get; set; } = null!; 
    public bool DeuxFA_Active { get; set; } 
    public string? DeuxFA_Secret { get; set; } 
    public DateTime? DerniereConnexion { get; set; } 
    public bool EstActif { get; set; } = true; 
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
    public DateTime? DateModification { get; set; } 
} 
