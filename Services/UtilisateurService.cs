using System.ComponentModel.DataAnnotations;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;

namespace AkarangaBank.Web.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AkarangaBankContext _context;

    public UtilisateurService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AkarangaBankContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .Include(user => user.Agence)
            .Include(user => user.Client)
            .OrderBy(user => user.NomComplet)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(int id)
    {
        return await _userManager.Users
            .Include(user => user.Agence)
            .Include(user => user.Client)
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
        if (!string.Equals(role, "client", StringComparison.OrdinalIgnoreCase))
            user.ClientId = null;
        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded ? (true, null) : (false, JoinErrors(updateResult));
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(UtilisateurEditViewModel vm)
    {
        var name = vm.NomComplet.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Le nom complet est obligatoire.");
        var email = (vm.Email ?? string.Empty).Trim();
        if (!new EmailAddressAttribute().IsValid(email))
            return (false, "Veuillez saisir une adresse e-mail valide.");
        if (!await _roleManager.RoleExistsAsync(vm.Role))
            return (false, $"Le rôle « {vm.Role} » n'existe pas.");
        if (vm.Role == "client" && vm.ClientId.HasValue
            && !await _context.Clients.AnyAsync(c => c.Id == vm.ClientId.Value))
            return (false, "Le dossier client sélectionné n'existe pas.");

        var user = await _userManager.FindByIdAsync(vm.Id.ToString());
        if (user is null)
            return (false, "Utilisateur introuvable.");

        var normalizedEmail = _userManager.NormalizeEmail(email);
        var normalizedUserName = _userManager.NormalizeName(email);
        if (await _userManager.Users.AnyAsync(other => other.Id != user.Id
            && (other.NormalizedEmail == normalizedEmail || other.NormalizedUserName == normalizedUserName)))
            return (false, "Cette adresse e-mail est déjà utilisée par un autre utilisateur.");

        if (vm.Role == "client" && vm.ClientId.HasValue
            && await _context.Users.AnyAsync(other => other.Id != user.Id && other.ClientId == vm.ClientId.Value))
            return (false, "Ce dossier client est déjà associé à un autre utilisateur.");

        var oldRoles = await _userManager.GetRolesAsync(user);
        var alreadyHasSelectedRole = oldRoles.Contains(vm.Role, StringComparer.OrdinalIgnoreCase);
        var rolesToRemove = oldRoles
            .Where(role => !string.Equals(role, vm.Role, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (!alreadyHasSelectedRole)
        {
            var addResult = await _userManager.AddToRoleAsync(user, vm.Role);
            if (!addResult.Succeeded)
                return (false, JoinErrors(addResult));
        }

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                if (!alreadyHasSelectedRole)
                    await _userManager.RemoveFromRoleAsync(user, vm.Role);
                return (false, JoinErrors(removeResult));
            }
        }

        var oldName = user.NomComplet;
        var oldEmail = user.Email;
        var oldUserName = user.UserName;
        var oldNormalizedEmail = user.NormalizedEmail;
        var oldNormalizedUserName = user.NormalizedUserName;
        var oldRole = user.Role;
        var oldClientId = user.ClientId;
        user.NomComplet = name;
        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = normalizedEmail;
        user.Role = vm.Role;
        user.ClientId = vm.Role == "client" ? vm.ClientId : null;
        user.DateModification = DateTime.UtcNow;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            user.NomComplet = oldName;
            user.Email = oldEmail;
            user.UserName = oldUserName;
            user.NormalizedEmail = oldNormalizedEmail;
            user.NormalizedUserName = oldNormalizedUserName;
            user.Role = oldRole;
            user.ClientId = oldClientId;
            if (!alreadyHasSelectedRole)
            {
                await _userManager.RemoveFromRoleAsync(user, vm.Role);
            }
            foreach (var oldRoleName in rolesToRemove)
                await _userManager.AddToRoleAsync(user, oldRoleName);
            return (false, JoinErrors(updateResult));
        }

        return (true, null);
    }

    private static string JoinErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
