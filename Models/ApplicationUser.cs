using Microsoft.AspNetCore.Identity; 
 
namespace AkarangaBank.Web.Models; 
 
public class ApplicationUser : IdentityUser<int> 
{ 
    public string NomComplet { get; set; } = string.Empty; 
    public string Role { get; set; } = "client"; 
    public int? AgenceId { get; set; } 
    public Agence Agence { get; set; } = null!; 
    public bool DeuxFA_Active { get; set; } 
    public string? DeuxFA_Secret { get; set; } 
    public DateTime? DerniereConnexion { get; set; } 
    public bool EstActif { get; set; } = true; 
 
    // Lien facultatif vers un Client (rôle client) 
    public int? ClientId { get; set; } 
    public Client? Client { get; set; } 
 
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
    public DateTime? DateModification { get; set; } 
} 