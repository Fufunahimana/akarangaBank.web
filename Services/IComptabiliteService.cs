using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IComptabiliteService
{
    Task<Operation> DeposerAsync(DepotViewModel vm, int utilisateurId);
    Task<Operation> RetirerAsync(RetraitViewModel vm, int utilisateurId);
    Task<Operation> VirerAsync(VirementViewModel vm, int utilisateurId);
    Task<IEnumerable<Operation>> GetOperationsAmlEnAttenteAsync();
    Task<bool> ValiderOperationAmlAsync(int operationId, int valideParId);
    Task<bool> RejeterOperationAmlAsync(int operationId, int valideParId, string motif);
    Task<JournalViewModel> GetJournalAsync(DateTime? du, DateTime? au, string? typeOperation, int page, int taille);
    Task<ReleveViewModel?> GetReleveAsync(int compteId);
    Task<decimal> GetSoldeAsync(int compteId);
    Task<bool> VerifierEquilibreGlobalAsync();
}
