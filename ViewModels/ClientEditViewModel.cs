using System.ComponentModel.DataAnnotations; 
 
namespace AkarangaBank.Web.ViewModels; 
 
public class ClientEditViewModel 
{ 
    public int Id { get; set; } 
 
    [Required] 
    [StringLength(20)] 
    [Display(Name = "Numéro client")] 
    public string NumeroClient { get; set; } = string.Empty; 
 
    [Required] 
    [StringLength(100)] 
    public string Nom { get; set; } = string.Empty; 
 
    [Required] 
    [StringLength(100)] 
    public string Prenom { get; set; } = string.Empty; 
 
    [DataType(DataType.Date)] 
    [Display(Name = "Date de naissance")] 
    public DateTime DateNaissance { get; set; } 
 
    [Required] 
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
 
    [Required] 
    [Display(Name = "Agence")] 
    public int AgenceId { get; set; } 
 
    [Required] 
    [StringLength(20)] 
    public string Statut { get; set; } = "Actif"; 
} 