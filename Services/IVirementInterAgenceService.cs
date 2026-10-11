using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public interface IVirementInterAgenceService
{
    Task<VirementInterAgence> InitierAsync(VirementInterAgenceViewModel vm, int utilisateurId);
    Task<bool> ValiderAsync(int virementInterAgenceId, int valideParId);
    Task<bool> RejeterAsync(int virementInterAgenceId, int valideParId, string motif);
    Task<IEnumerable<VirementInterAgence>> GetEnAttenteAsync(int? agenceId, int? utilisateurId = null);
    Task<IEnumerable<VirementInterAgence>> GetHistoriqueAsync(
        int? agenceId, int? utilisateurId, DateTime? du, DateTime? au);
}
