using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class CarteEmissionViewModel
{
    [Required]
    public int CompteId { get; set; }

    [Required]
    public string TypeCarte { get; set; } = "Visa";

    [Range(typeof(decimal), "1", "9999999999999999")]
    public decimal PlafondRetraitJournalier { get; set; }

    [Range(typeof(decimal), "1", "9999999999999999")]
    public decimal PlafondPaiementJournalier { get; set; }

    [Range(typeof(decimal), "1", "9999999999999999")]
    public decimal PlafondPaiementEnLigneJournalier { get; set; }
}
