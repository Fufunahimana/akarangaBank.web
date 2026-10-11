using System.ComponentModel.DataAnnotations; 
using AkarangaBank.Web.Services;
 
namespace AkarangaBank.Web.ViewModels; 
 
public class ClientCreateViewModel 
{ 
    [Required(ErrorMessage = "Le numéro de client est obligatoire.")] 
    [StringLength(20)] 
    [Display(Name = "Numéro client")] 
    [RegularExpression(@"^CLI-\d{6}$", 
        ErrorMessage = "Le numéro doit respecter le format CLINNNNNN.")] 
    public string NumeroClient { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "Le nom est obligatoire.")] 
    [StringLength(100)] 
    public string Nom { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "Le prénom est obligatoire.")] 
    [StringLength(100)] 
    public string Prenom { get; set; } = string.Empty; 
 
    [Required(ErrorMessage = "La date de naissance est obligatoire.")] 
    [DataType(DataType.Date)] 
    [Display(Name = "Date de naissance")] 
    public DateTime DateNaissance { get; set; } = 
BankTime.Today.AddYears(-18); 
 
    [Required(ErrorMessage = "Le numéro de CNI est obligatoire.")] 
    [StringLength(30)] 
    [Display(Name = "Numéro CNI")] 
    public string NumeroCNI { get; set; } = string.Empty; 
 
    [Phone] 
    [StringLength(20)] 
    public string? Telephone { get; set; } 
 
    [EmailAddress] 
    [StringLength(100)]   
    public string? Email { get; set; } 
 
    [StringLength(255)] 
    public string? Adresse { get; set; } 
 
    [Required(ErrorMessage = "L'agence est obligatoire.")] 
    [Display(Name = "Agence")] 
    public int AgenceId { get; set; } 
} 
