using System.ComponentModel.DataAnnotations;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.ViewModels; 
 
public class VerificationKYCCreateViewModel 
{ 
    [Required] 
    public int ClientId { get; set; } 
 
    [Required(ErrorMessage = "Le type de document est obligatoire.")] 
    [StringLength(50)] 
    [Display(Name = "Type de document")]  
    public string TypeDocument { get; set; } = "CNI"; 
 
    [Required(ErrorMessage = "Le numéro de document est obligatoire.")] 
    [StringLength(50)] 
    [Display(Name = "Numéro de document")] 
    public string NumeroDocument { get; set; } = string.Empty; 
 
    [StringLength(500)] 
    [Display(Name = "Chemin du document")] 
    public string? CheminDocument { get; set; } 
 
    [StringLength(500)] 
    public string? Commentaire { get; set; } 
 
    // Pour l'affichage 
    public string? ClientNom { get; set; } 
    public string? ClientNumero { get; set; } 
}