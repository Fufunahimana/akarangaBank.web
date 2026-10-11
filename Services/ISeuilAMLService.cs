using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.Services;

public interface ISeuilAMLService
{
    Task<SeuilAML?> GetSeuilAsync(string code, string typeOperation);
    Task<IEnumerable<SeuilAML>> GetAllAsync();
    Task<bool> MettreAJourAsync(int seuilId, decimal nouveauMontant, int utilisateurId);
    Task<decimal> GetSeuilVigilanceAsync();
    Task<decimal> GetSeuilDeclarationAsync(string typeOperation = "Toutes");
    Task<decimal> GetSeuilBlocageAsync();
}
