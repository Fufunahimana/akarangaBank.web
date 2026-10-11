using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface ICarteService
{
    Task<Carte> EmettreAsync(CarteEmissionViewModel demande);
    Task<Carte> BloquerAsync(int carteId, string motif);
    Task<Carte> RenouvelerAsync(int carteId);
    Task<TransactionCarte> AutoriserTransactionAsync(int carteId, decimal montant, string typeTransaction, int utilisateurId, DateTime? maintenant = null);
    Task<IReadOnlyList<TransactionCarte>> GetHistoriqueAsync(int carteId);
}
