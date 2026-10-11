using System.ComponentModel.DataAnnotations; 
 
namespace AkarangaBank.Web.ViewModels; 
 
public class CompteEditViewModel 
{ 
    public int Id { get; set; } 
 
    [Required] 
    [StringLength(30)] 
    [Display(Name = "Numéro de compte")] 
    public string NumeroCompte { get; set; } = string.Empty; 
 
    [Required] 
    [Display(Name = "Type de compte")] 
    public int TypeCompteId { get; set; } 
 
    [Required] 
    [Display(Name = "Agence")] 
    public int AgenceId { get; set; } 
 
    [Required] 
    [StringLength(20)] 
    public string Statut { get; set; } = "Actif"; 
 public string? ClientNom { get; set; } 
    public decimal Solde { get; set; } 
    public string Devise { get; set; } = "BIF"; 
} 
