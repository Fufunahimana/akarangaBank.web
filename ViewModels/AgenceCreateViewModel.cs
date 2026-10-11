using System.ComponentModel.DataAnnotations; 
 
namespace AkarangaBank.Web.ViewModels; 
 
// Données nécessaires pour créer une agence. 
// Les Data Annotations définissent les règles de validation côté serveur (via ModelState.IsValid)
// ET côté client (via jQuery Validation). 
public class AgenceCreateViewModel 
{ 
    [Required(ErrorMessage = "Le code de l'agence est obligatoire.")] 
    [StringLength(20, MinimumLength = 3, 
        ErrorMessage = "Le code doit contenir entre 3 et 20 caractères.")] 
    [Display(Name = "Code agence")] 
    [RegularExpression(@"^AKR-[A-Za-z0-9]{3}$", 
        ErrorMessage = "Utilisez le format AKR-XXX avec 3 lettres ou chiffres après le tiret.")]
    public string Code { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "Le nom est obligatoire.")] 
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")] 
    public string Nom { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "La ville est obligatoire.")] 
    [StringLength(50)] 
    public string Ville { get; set; } = string.Empty; 
 
    [Display(Name = "Est le siège")] 
    public bool EstSiege { get; set; } 
} 
