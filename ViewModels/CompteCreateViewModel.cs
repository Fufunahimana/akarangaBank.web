using System.ComponentModel.DataAnnotations; 
 
namespace AkarangaBank.Web.ViewModels; 
 
public class CompteCreateViewModel 
{ 
    [Required(ErrorMessage = "Le numéro de compte est obligatoire.")] 
    [StringLength(30)] 
    [Display(Name = "Numéro de compte")] 
    [RegularExpression(@"^[A-Z]{3}-\d{4}-\d{4}$", 
        ErrorMessage = "Le numéro doit respecter le format XXX-NNNN-NNNN.")] 
    public string NumeroCompte { get; set; } = string.Empty; 
 
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un client valide.")]
 
    [Display(Name = "Client")] 
    public int ClientId { get; set; } 
 
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un type de compte valide.")]
    [Display(Name = "Type de compte")] 
    public int TypeCompteId { get; set; } 
 
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner une agence valide.")]
    [Display(Name = "Agence")] 
    public int AgenceId { get; set; } 
 
    [Range(0, double.MaxValue, ErrorMessage = "Le solde initial doit être positif ou nul.")] 
    [Display(Name = "Solde initial (BIF)")] 
    public decimal SoldeInitial { get; set; } 
} 
