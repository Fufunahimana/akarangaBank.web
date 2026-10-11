using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface IDeclarationService
{
    Task<AlerteAML> CreerAlerteAsync(int clientId, int? operationId, string typeAlerte, string niveau, string description);
    Task<bool> TraiterAlerteAsync(int alerteId, int traiteParId, string statut, string? commentaire);
    Task<IEnumerable<AlerteAML>> GetAlertesAsync(string? statut, string? niveau);
}
