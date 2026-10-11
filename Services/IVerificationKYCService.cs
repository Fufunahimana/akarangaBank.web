using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Services; 
 
public interface IVerificationKYCService 
{ 
    Task<VerificationKYC> CreateAsync(VerificationKYCCreateViewModel 
vm, int? verifieParId); 
    Task<bool> ValiderAsync(int id, int verifieParId); 
    Task<bool> RejeterAsync(int id, int verifieParId, string commentaire); 
    Task<VerificationKYC?> GetByIdAsync(int id); 
} 