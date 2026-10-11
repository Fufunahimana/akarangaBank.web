using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Services; 
 
public interface ICompteService 
{ 
    Task<CompteListViewModel> GetPagedAsync( 
        string? recherche, int? agenceId, int? typeCompteId, string? 
statut, int page, int taille); 
    Task<CompteDetailsViewModel?> GetDetailsAsync(int id); 
    Task<Compte?> GetByIdAsync(int id); 
    Task<Compte> CreateAsync(CompteCreateViewModel vm); 
    Task<bool> UpdateAsync(CompteEditViewModel vm); 
    Task<bool> DeleteAsync(int id); 
    Task<bool> NumeroCompteExisteAsync(string numeroCompte, int? 
excludeId = null); 
}