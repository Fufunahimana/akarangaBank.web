using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class CompteDetailsViewModel
{
    public Compte Compte { get; set; } = null!;
    public IEnumerable<Ecriture> Ecritures { get; set; } = new List<Ecriture>(); // <-- S'assure que Ecriture existe dans Models
}