using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class SeuilsAMLController : Controller
{
    private readonly ISeuilAMLService _service;
    private readonly ICurrentUserService _currentUser;

    public SeuilsAMLController(ISeuilAMLService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modifier(int id, decimal montant)
    {
        if (montant <= 0)
        {
            TempData["Error"] = "Le seuil doit être supérieur à zéro.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var modifie = await _service.MettreAJourAsync(id, montant, await _currentUser.GetUserIdAsync());
            TempData[modifie ? "Success" : "Error"] = modifie ? "Seuil AML mis à jour. La modification a été inscrite dans l'audit." : "Seuil AML introuvable.";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
