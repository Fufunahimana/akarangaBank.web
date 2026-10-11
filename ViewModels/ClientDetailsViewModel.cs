using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public class ClientDetailsViewModel
{
    public Client Client { get; set; } = null!;
    public IEnumerable<Compte> Comptes { get; set; } = new List<Compte>();
    public IEnumerable<VerificationKYC> VerificationsKYC { get; set; } = new List<VerificationKYC>(); // <-- S'assure que VerificationKYC existe dans Models
    public decimal SoldeTotal => Comptes.Sum(c => c.Solde);
}