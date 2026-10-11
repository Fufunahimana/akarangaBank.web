using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class PretDemandeViewModel
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int CompteDebitId { get; set; }

    [Required]
    public int TypePretId { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal MontantPrincipal { get; set; }

    [Range(1, 600)]
    public int DureeMois { get; set; }

    [StringLength(1000)]
    public string? Commentaire { get; set; }
}
