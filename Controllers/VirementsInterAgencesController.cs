using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "OperationsWrite")]
public class VirementsInterAgencesController : Controller
{
    private readonly IVirementInterAgenceService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AkarangaBankContext _context;

    public VirementsInterAgencesController(
        IVirementInterAgenceService service,
        ICurrentUserService currentUser,
        AkarangaBankContext context)
    {
        _service = service;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var agenceId = await GetAgenceFiltrageAsync();
        int? utilisateurId = User.IsInRole("admin") ? null : await _currentUser.GetUserIdAsync();
        return View(await _service.GetEnAttenteAsync(agenceId, utilisateurId));
    }

    [HttpGet]
    public async Task<IActionResult> Initier()
    {
        await ChargerComptesAsync();
        return View(new VirementInterAgenceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Initier(VirementInterAgenceViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }

        try
        {
            var transfert = await _service.InitierAsync(vm, await _currentUser.GetUserIdAsync());
            TempData["Success"] = $"Virement inter-agence initié (réf. {transfert.Reference}).";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Valider(int id)
    {
        try
        {
            var valide = await _service.ValiderAsync(id, await _currentUser.GetUserIdAsync());
            TempData[valide ? "Success" : "Error"] = valide ? "Virement validé et comptabilisé." : "Ce virement n'est plus en attente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Rejeter(int id, string motif)
    {
        var rejete = await _service.RejeterAsync(id, await _currentUser.GetUserIdAsync(), motif);
        TempData[rejete ? "Success" : "Error"] = rejete ? "Virement rejeté." : "Ce virement n'est plus en attente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Historique(DateTime? du, DateTime? au)
    {
        int? utilisateurId = User.IsInRole("admin") ? null : await _currentUser.GetUserIdAsync();
        var virements = await _service.GetHistoriqueAsync(
            await GetAgenceFiltrageAsync(), utilisateurId, du, au);
        ViewBag.Du = du;
        ViewBag.Au = au;
        return View(virements);
    }

    private async Task<int?> GetAgenceIdAsync()
    {
        var id = await _currentUser.GetUserIdAsync();
        return await _context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => u.AgenceId)
            .FirstOrDefaultAsync();
    }

    private async Task<int?> GetAgenceFiltrageAsync()
    {
        if (User.IsInRole("admin"))
            return null;

        // null means unrestricted access in the service and is reserved for admins.
        return await GetAgenceIdAsync() ?? int.MinValue;
    }

    private async Task ChargerComptesAsync(int? sourceId = null, int? destinationId = null)
    {
        var query = _context.Comptes.AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Agence)
            .Where(c => c.Statut == "Actif" && !c.NumeroCompte.StartsWith("9000"));

        var comptes = await query
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new { c.Id, c.AgenceId, Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Agence.Code})" })
            .ToListAsync();
        ViewBag.ComptesSource = new SelectList(comptes, "Id", "Libelle", sourceId);
        ViewBag.ComptesDestination = new SelectList(comptes, "Id", "Libelle", destinationId);
    }
}
