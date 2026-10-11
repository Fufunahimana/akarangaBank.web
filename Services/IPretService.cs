using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IPretService
{
    decimal CalculerMensualite(decimal principal, decimal tauxAnnuel, int dureeMois);
    IReadOnlyList<EcheancePret> GenererEcheancier(Pret pret, DateTime? dateDebut = null);
    Task<Pret> CreerDemandeAsync(PretDemandeViewModel demande);
    Task<Pret> ValiderAsync(int pretId, decimal tauxPersonnalise, int utilisateurId);
    Task<Pret> RejeterAsync(int pretId, string motif);
    Task<Operation> DebloquerAsync(int pretId, int utilisateurId);
    Task<Operation> RembourserAsync(int echeanceId, int utilisateurId, DateTime? maintenant = null);
    Task<int> MettreAJourRetardsAsync(DateTime? maintenant = null);
}
