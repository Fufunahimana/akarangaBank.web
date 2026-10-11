using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class VirementInterAgenceViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Compte source")]
    public int CompteSourceId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Compte destination")]
    public int CompteDestinationId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Montant { get; set; }

    [StringLength(500)]
    public string? Motif { get; set; }
}
