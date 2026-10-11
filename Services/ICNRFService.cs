using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface ICNRFService
{
    Task<DeclarationSoupcon> GenererDOSAsync(int alerteId, int redigeParId);
    Task<bool> TransmettreAsync(int declarationId);
    Task<bool> EnregistrerAccuseAsync(int declarationId, string reponse);
    Task<IEnumerable<DeclarationSoupcon>> GetDeclarationsAsync(string? statut);
    Task<string> GenererContenuDOSAsync(AlerteAML alerte, string? reference = null);
}
