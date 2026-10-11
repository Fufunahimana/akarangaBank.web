using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class CNRFController : Controller
{
    private readonly ICNRFService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AkarangaBankContext _context;

    public CNRFController(ICNRFService service, ICurrentUserService currentUser, AkarangaBankContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<IActionResult> Index(string? statut)
    {
        ViewBag.Statut = statut;
        ViewBag.Alertes = await _context.AlertesAML.AsNoTracking()
            .Include(a => a.Client)
            .Where(a => !_context.DeclarationsSoupcon.Any(d => d.AlerteId == a.Id && d.TypeDeclaration == "DOS"))
            .OrderByDescending(a => a.DateAlerte)
            .ToListAsync();
        return View(await _service.GetDeclarationsAsync(statut));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generer(int alerteId)
    {
        try
        {
            var declaration = await _service.GenererDOSAsync(alerteId, await _currentUser.GetUserIdAsync());
            TempData["Success"] = $"DOS générée : {declaration.Reference}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transmettre(int id)
    {
        var transmise = await _service.TransmettreAsync(id);
        TempData[transmise ? "Success" : "Error"] = transmise
            ? "Déclaration marquée comme transmise (mode de démonstration locale)."
            : "Cette déclaration ne peut pas être transmise dans son état actuel.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var declaration = (await _service.GetDeclarationsAsync(null)).FirstOrDefault(d => d.Id == id);
        return declaration is null ? NotFound() : View(declaration);
    }
}
