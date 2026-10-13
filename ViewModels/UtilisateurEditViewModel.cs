using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public sealed class UtilisateurEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nom complet")]
    public string NomComplet { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Adresse e-mail")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Rôle")]
    public string Role { get; set; } = "client";

    [Display(Name = "Dossier client associé (facultatif)")]
    public int? ClientId { get; set; }
}
