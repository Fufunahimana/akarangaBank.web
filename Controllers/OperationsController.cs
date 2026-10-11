using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize]
public class OperationsController : Controller
{
    private readonly IComptabiliteService _comptabilite;
    private readonly IVirementInterAgenceService _virementsInterAgences;
    private readonly ICurrentUserService _currentUser;
    private readonly AkarangaBankContext _context;

    public OperationsController(
        IComptabiliteService comptabilite,
        IVirementInterAgenceService virementsInterAgences,
        ICurrentUserService currentUser,
        AkarangaBankContext context)
    {
        _comptabilite = comptabilite;
        _virementsInterAgences = virementsInterAgences;
        _currentUser = currentUser;
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Depot()
    {
        await ChargerComptesAsync();
        return View(new DepotViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Depot(DepotViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }

        try
        {
            var operation = await _comptabilite.DeposerAsync(vm, await _currentUser.GetUserIdAsync());
            TempData["Success"] = operation.Statut == "EnAttente"
                ? $"Dépôt en attente d'approbation admin (réf. {operation.Reference})."
                : $"Dépôt enregistré (réf. {operation.Reference}).";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    [HttpGet]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Retrait()
    {
        await ChargerComptesAsync();
        return View(new RetraitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Retrait(RetraitViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }

        try
        {
            var operation = await _comptabilite.RetirerAsync(vm, await _currentUser.GetUserIdAsync());
            TempData["Success"] = operation.Statut == "EnAttente"
                ? $"Retrait en attente d'approbation admin (réf. {operation.Reference})."
                : $"Retrait enregistré (réf. {operation.Reference}).";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    [HttpGet]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Virement()
    {
        await ChargerComptesVirementAsync();
        return View(new VirementViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Virement(VirementViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesVirementAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }

        try
        {
            var agencesComptes = await _context.Comptes.AsNoTracking()
                .Where(c => c.Id == vm.CompteSourceId || c.Id == vm.CompteDestinationId)
                .Select(c => new { c.Id, c.AgenceId })
                .ToDictionaryAsync(c => c.Id, c => c.AgenceId);

            if (agencesComptes.TryGetValue(vm.CompteSourceId, out var agenceSourceId)
                && agencesComptes.TryGetValue(vm.CompteDestinationId, out var agenceDestinationId)
                && agenceSourceId != agenceDestinationId)
            {
                var userId = await _currentUser.GetUserIdAsync();
                var transfert = await _virementsInterAgences.InitierAsync(
                    new VirementInterAgenceViewModel
                    {
                        CompteSourceId = vm.CompteSourceId,
                        CompteDestinationId = vm.CompteDestinationId,
                        Montant = vm.Montant,
                        Motif = vm.Motif
                    },
                    userId);

                TempData["Success"] = $"Virement inter-agence enregistré en attente d'approbation administrateur (réf. {transfert.Reference}).";
                return RedirectToAction("Index", "VirementsInterAgences");
            }

            var operation = await _comptabilite.VirerAsync(vm, await _currentUser.GetUserIdAsync());
            TempData["Success"] = $"Virement enregistré (réf. {operation.Reference}).";
            return RedirectToAction(nameof(Journal));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesVirementAsync(vm.CompteSourceId, vm.CompteDestinationId);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Journal(DateTime? du, DateTime? au, string? typeOperation, int page = 1)
    {
        var vm = await _comptabilite.GetJournalAsync(du, au, typeOperation, page, 20);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> EnAttenteValidation()
    {
        return View(await _comptabilite.GetOperationsAmlEnAttenteAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ValiderAml(int id)
    {
        try
        {
            var validee = await _comptabilite.ValiderOperationAmlAsync(id, await _currentUser.GetUserIdAsync());
            TempData[validee ? "Success" : "Error"] = validee
                ? "Opération AML approuvée et comptabilisée."
                : "Cette opération n'est plus en attente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(EnAttenteValidation));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RejeterAml(int id, string motif)
    {
        try
        {
            var rejetee = await _comptabilite.RejeterOperationAmlAsync(id, await _currentUser.GetUserIdAsync(), motif);
            TempData[rejetee ? "Success" : "Error"] = rejetee
                ? "Opération AML rejetée."
                : "Cette opération n'est plus en attente.";
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(EnAttenteValidation));
    }

    [HttpGet]
    public async Task<IActionResult> Releve(int id)
    {
        var vm = await _comptabilite.GetReleveAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet]
    [Authorize(Policy = BankAuthorizationPolicies.OperationsWrite)]
    public async Task<IActionResult> Details(int id)
    {
        var operation = await _context.Operations.AsNoTracking()
            .Include(o => o.Compte).ThenInclude(c => c.Client)
            .Include(o => o.Ecritures).ThenInclude(e => e.Compte)
            .FirstOrDefaultAsync(o => o.Id == id);
        return operation is null ? NotFound() : View(operation);
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ControleEquilibre()
    {
        ViewBag.Equilibre = await _comptabilite.VerifierEquilibreGlobalAsync();
        return View();
    }

    private async Task ChargerComptesAsync(int? sourceId = null, int? destinationId = null)
    {
        var comptes = await _context.Comptes.AsNoTracking()
            .Include(c => c.Client)
            .Where(c => c.Statut == "Actif")
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new
            {
                c.Id,
                Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Solde:N2} {c.Devise})"
            })
            .ToListAsync();

        ViewBag.Comptes = new SelectList(comptes, "Id", "Libelle", sourceId);
        ViewBag.ComptesDestination = new SelectList(comptes, "Id", "Libelle", destinationId);
    }

    private async Task ChargerComptesVirementAsync(int? sourceId = null, int? destinationId = null)
    {
        var comptes = await _context.Comptes.AsNoTracking()
            .Include(c => c.Client)
            .Where(c => c.Statut == "Actif")
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new
            {
                c.Id,
                c.AgenceId,
                Libelle = $"{c.NumeroCompte} — {c.Client.Nom} {c.Client.Prenom} ({c.Solde:N2} {c.Devise})"
            })
            .ToListAsync();

        ViewBag.Comptes = new SelectList(comptes, "Id", "Libelle", sourceId);
        ViewBag.ComptesDestination = new SelectList(comptes, "Id", "Libelle", destinationId);
    }
}
