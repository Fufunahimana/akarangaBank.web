using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Services; 
 
public interface IClientService 
{ 
    Task<ClientListViewModel> GetPagedAsync(string? recherche, int? 
agenceId, string? statut, int page, int taille); 
    Task<ClientDetailsViewModel?> GetDetailsAsync(int id); 
    Task<Client?> GetByIdAsync(int id); 
    Task<Client> CreateAsync(ClientCreateViewModel vm); 
    Task<bool> UpdateAsync(ClientEditViewModel vm); 
    Task<bool> DeleteAsync(int id); 
    Task<bool> NumeroClientExisteAsync(string numeroClient, int? 
excludeId = null); 
    Task<bool> NumeroCNIExisteAsync(string numeroCNI, int? excludeId 
= null); 
} 