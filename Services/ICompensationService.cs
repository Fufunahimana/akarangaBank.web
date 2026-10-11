using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface ICompensationService
{
    Task<CompensationAgence> CalculerCompensationAsync(int agenceId, DateTime date);
    Task<bool> ReglerCompensationAsync(int compensationId, int valideParId);
    Task<IEnumerable<CompensationAgence>> GetCompensationsAsync(int? agenceId, DateTime? date);
    Task<bool> VerifierEquilibreCompensationsAsync(DateTime date);
}
