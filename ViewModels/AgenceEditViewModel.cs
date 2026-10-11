using System.ComponentModel.DataAnnotations; 
 
namespace AkarangaBank.Web.ViewModels; 
 
public class AgenceEditViewModel 
{ 
    public int Id { get; set; } 
 
    [Required(ErrorMessage = "Le code de l'agence est obligatoire.")
] 
    [StringLength(20, MinimumLength = 3)] 
    [Display(Name = "Code agence")] 
    [RegularExpression(@"^AKR-[A-Za-z0-9]{3}$", 
        ErrorMessage = "Utilisez le format AKR-XXX avec 3 lettres ou chiffres après le tiret.")] 
    public string Code { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "Le nom est obligatoire.")] 
    [StringLength(100)] 
    public string Nom { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "La ville est obligatoire.")] 
    [StringLength(50)] 
    public string Ville { get; set; } = string.Empty; 
 
    [Display(Name = "Est le siège")] 
    public bool EstSiege { get; set; }  
}
