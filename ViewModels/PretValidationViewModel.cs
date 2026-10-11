using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class PretValidationViewModel
{
    public int PretId { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TauxInteret { get; set; }
}
