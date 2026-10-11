using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UtilisateurService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .Include(user => user.Agence)
            .OrderBy(user => user.NomComplet)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id)
    {
        return await _userManager.Users
            .Include(user => user.Agence)
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(UtilisateurCreateViewModel vm)
    {
        if (!await _roleManager.RoleExistsAsync(vm.Role))
            return (false, $"Le rôle « {vm.Role} » n'existe pas.");

        var user = new ApplicationUser
        {
            UserName = vm.Email.Trim(),
            Email = vm.Email.Trim(),
            NomComplet = vm.NomComplet.Trim(),
            Role = vm.Role,
            AgenceId = vm.AgenceId,
            EstActif = true
        };

        var createResult = await _userManager.CreateAsync(user, vm.Password);
        if (!createResult.Succeeded)
            return (false, JoinErrors(createResult));

        var roleResult = await _userManager.AddToRoleAsync(user, vm.Role);
        if (roleResult.Succeeded)
            return (true, null);

        // Évite de conserver un compte créé sans le rôle demandé.
        await _userManager.DeleteAsync(user);
        return (false, JoinErrors(roleResult));
    }

    public async Task<(bool Success, string? Error)> ToggleActifAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return (false, "Utilisateur introuvable.");

        user.EstActif = !user.EstActif;
        user.DateModification = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? (true, null) : (false, JoinErrors(result));
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(int id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return (false, "Utilisateur introuvable.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded ? (true, null) : (false, JoinErrors(result));
    }

    public async Task<(bool Success, string? Error)> AssignRoleAsync(int id, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            return (false, $"Le rôle « {role} » n'existe pas.");

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return (false, "Utilisateur introuvable.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            return (true, null);

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
            return (false, JoinErrors(addResult));

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
                return (false, JoinErrors(removeResult));
            }
        }

        user.Role = role;
        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded ? (true, null) : (false, JoinErrors(updateResult));
    }

    private static string JoinErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
