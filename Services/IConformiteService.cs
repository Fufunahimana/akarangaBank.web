using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IConformiteService
{
    Task<ResultatConformite> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation);
    Task<bool> DetecterFractionnementAsync(int compteId, decimal montant, DateTime date);
    Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId);
    Task<decimal> GetSeuilAMLAsync();
}
