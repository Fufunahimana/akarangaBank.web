using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace AkarangaBank.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserService(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager)
    {
        _accessor = accessor;
        _userManager = userManager;
    }

    public async Task<int> GetUserIdAsync()
    {
        var principal = _accessor.HttpContext?.User
            ?? throw new InvalidOperationException("Aucun utilisateur connecté.");
        var user = await _userManager.GetUserAsync(principal);
        return user?.Id ?? throw new InvalidOperationException("Utilisateur non connecté.");
    }

    public async Task<int> GetAgenceIdAsync()
    {
        var principal = _accessor.HttpContext?.User
            ?? throw new InvalidOperationException("Aucun utilisateur connecté.");
        var user = await _userManager.GetUserAsync(principal);
        return user?.AgenceId ?? throw new InvalidOperationException("Aucune agence associée à cet utilisateur.");
    }
}
