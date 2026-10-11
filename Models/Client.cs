using System.Collections.Generic;
using AkarangaBank.Web.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkarangaBank.Web.Models; 
 
public class Client 
{ 
    public int Id { get; set; }  
    public string NumeroClient { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty; 
    public string Prenom { get; set; } = string.Empty; 
    public DateTime DateNaissance { get; set; } 
    public string NumeroCNI { get; set; } = string.Empty; 
    public string? Telephone { get; set; } 
    public string? Email { get; set; } 
    public string? Adresse { get; set; } 
    public int AgenceId { get; set; } 
    public Agence Agence { get; set; } = null!; 
    public string Statut { get; set; } = "Actif"; 
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
    public DateTime? DateModification { get; set; } 
 
    // Navigation 
    public ICollection<Compte> Comptes { get; set; } = new List<Compte>(); 
    public ICollection<VerificationKYC> VerificationsKYC { get; set; } = new List<VerificationKYC>(); 
    
}