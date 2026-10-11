using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels;
using AkarangaBank.Web.Services;
 
namespace AkarangaBank.Web.Services; 
 
public interface IUtilisateurService 
{ 
    Task<IEnumerable<ApplicationUser>> GetAllAsync(); 
    Task<ApplicationUser?> GetByIdAsync(int id); 
    Task<(bool Success, string? Error)> 
CreateAsync(UtilisateurCreateViewModel vm); 
    Task<(bool Success, string? Error)> ToggleActifAsync(int id); 
    Task<(bool Success, string? Error)> ResetPasswordAsync(int id, string newPassword); 
    Task<(bool Success, string? Error)> AssignRoleAsync(int id, string role); 
} 
