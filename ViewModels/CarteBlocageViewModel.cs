using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class CarteBlocageViewModel
{
    public int CarteId { get; set; }

    [Required]
    public string Motif { get; set; } = string.Empty;
}
