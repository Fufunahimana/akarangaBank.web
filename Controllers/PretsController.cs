using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize]
public class PretsController : Controller
{
    private readonly AkarangaBankContext _context;
    private readonly IPretService _prets;
    private readonly ICurrentUserService _currentUser;

    public PretsController(AkarangaBankContext context, IPretService prets, ICurrentUserService currentUser)
    {
        _context = context;
        _prets = prets;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var query = _context.Prets.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.TypePret)
            .Include(p => p.CompteDebit)
            .OrderByDescending(p => p.DateDemande);
        if (User.IsInRole("client"))
        {
            var userId = await _currentUser.GetUserIdAsync();
            var clientId = await _context.Users.Where(u => u.Id == userId)
                .Select(u => u.ClientId).FirstOrDefaultAsync();
            if (!clientId.HasValue) return View(new List<Pret>());
            return View(await query.Where(p => p.ClientId == clientId.Value).ToListAsync());
        }
        if (!User.IsInRole("admin") && !User.IsInRole("gestionnaire") && !User.IsInRole("guichetier") && !User.IsInRole("auditeur"))
            return Forbid();
        return View(await query.ToListAsync());
    }

    [HttpGet]
    [Authorize(Policy = "GuichetierOnly")]
    public async Task<IActionResult> Create()
    {
        await ChargerListesAsync();
        return View(new PretDemandeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GuichetierOnly")]
    public async Task<IActionResult> Create(PretDemandeViewModel demande)
    {
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(demande);
            return View(demande);
        }
        try
        {
            var pret = await _prets.CreerDemandeAsync(demande);
            TempData["Success"] = $"Demande de prêt enregistrée : {pret.Reference}. Mensualité estimée : {pret.Mensualite:N2} BIF.";
            return RedirectToAction(nameof(Details), new { id = pret.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerListesAsync(demande);
            return View(demande);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var pret = await ChargerPretAsync(id);
        if (pret is null) return NotFound();
        if (!await ALeDroitSurPretAsync(pret, operationFinanciere: false)) return Forbid();
        return View(pret);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionnaireOrAdmin")]
    public async Task<IActionResult> Valider(PretValidationViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Le taux proposé est invalide.";
            return RedirectToAction(nameof(Details), new { id = vm.PretId });
        }
        try
        {
            await _prets.ValiderAsync(vm.PretId, vm.TauxInteret, await _currentUser.GetUserIdAsync());
            TempData["Success"] = "Le prêt est validé. Vous pouvez maintenant le débloquer.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id = vm.PretId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionnaireOrAdmin")]
    public async Task<IActionResult> Rejeter(PretRejetViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Le motif de rejet est obligatoire.";
            return RedirectToAction(nameof(Details), new { id = vm.PretId });
        }
        try
        {
            await _prets.RejeterAsync(vm.PretId, vm.Motif);
            TempData["Success"] = "La demande de prêt a été rejetée.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id = vm.PretId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Debloquer(int id)
    {
        try
        {
            var operation = await _prets.DebloquerAsync(id, await _currentUser.GetUserIdAsync());
            TempData["Success"] = $"Prêt débloqué. Référence comptable : {operation.Reference}.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rembourser(int echeanceId)
    {
        var pret = await _context.EcheancesPret.Where(e => e.Id == echeanceId)
            .Select(e => e.Pret).FirstOrDefaultAsync();
        if (pret is null) return NotFound();
        if (!await ALeDroitSurPretAsync(pret, operationFinanciere: true)) return Forbid();
        try
        {
            var operation = await _prets.RembourserAsync(echeanceId, await _currentUser.GetUserIdAsync());
            TempData["Success"] = $"Remboursement enregistré : {operation.Montant:N2} BIF.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id = pret.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> MettreAJourRetards()
    {
        var nombre = await _prets.MettreAJourRetardsAsync();
        TempData["Success"] = $"Mise à jour terminée : {nombre} échéance(s) en retard.";
        return RedirectToAction(nameof(Index));
    }

    private Task<Pret?> ChargerPretAsync(int id) => _context.Prets.AsNoTracking()
        .Include(p => p.Client)
        .Include(p => p.TypePret)
        .Include(p => p.CompteDebit)
        .Include(p => p.Echeances.OrderBy(e => e.NumeroEcheance))
        .FirstOrDefaultAsync(p => p.Id == id);

    private async Task<bool> ALeDroitSurPretAsync(Pret pret, bool operationFinanciere)
    {
        if (User.IsInRole("admin") || User.IsInRole("gestionnaire") || User.IsInRole("guichetier")) return true;
        if (User.IsInRole("auditeur")) return !operationFinanciere;
        if (!User.IsInRole("client")) return false;
        var userId = await _currentUser.GetUserIdAsync();
        var clientId = await _context.Users.Where(u => u.Id == userId).Select(u => u.ClientId).FirstOrDefaultAsync();
        return clientId == pret.ClientId;
    }

    private async Task ChargerListesAsync(PretDemandeViewModel? vm = null)
    {
        var clients = await _context.Clients.AsNoTracking().Where(c => c.Statut == "Actif")
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .Select(c => new { c.Id, Libelle = c.NumeroClient + " — " + c.Nom + " " + c.Prenom }).ToListAsync();
        var comptes = await _context.Comptes.AsNoTracking().Include(c => c.Client)
            .Where(c => c.Statut == "Actif")
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new { c.Id, Libelle = c.NumeroCompte + " — " + c.Client.Nom + " " + c.Client.Prenom }).ToListAsync();
        var types = await _context.TypesPret.AsNoTracking().Where(t => t.Actif).OrderBy(t => t.Libelle).ToListAsync();
        ViewBag.Clients = new SelectList(clients, "Id", "Libelle", vm?.ClientId);
        ViewBag.Comptes = new SelectList(comptes, "Id", "Libelle", vm?.CompteDebitId);
        ViewBag.TypesPret = new SelectList(types, "Id", "Libelle", vm?.TypePretId);
    }
}
