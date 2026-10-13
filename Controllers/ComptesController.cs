using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector; // Namespace pour les exceptions MariaDB/MySQL
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using AkarangaBank.Web.Data;

namespace AkarangaBank.Web.Controllers;

public class ComptesController : Controller {
    private readonly ICompteService _compteService;
    private readonly IClientService _clientService;
    private readonly IAgenceService _agenceService;
    private readonly AkarangaBankContext _context;
    private readonly ICurrentUserService _currentUser;

    public ComptesController(
        ICompteService compteService, 
        IClientService clientService, 
        IAgenceService agenceService,
        AkarangaBankContext context,
        ICurrentUserService currentUser) {
        _compteService = compteService;
        _clientService = clientService;
        _agenceService = agenceService;
        _context = context;
        _currentUser = currentUser;
    }

    // GET: /Comptes
    public async Task<IActionResult> Index(
        string? recherche, int? agenceId, int? typeCompteId, string? statut, int page = 1) {
        const int taille = 10;
        int? clientId = null;
        if (User.IsInRole("client"))
        {
            clientId = await GetClientIdAsync();
            if (!clientId.HasValue || !await AUnCompteActifAsync(clientId.Value))
                return RedirectToAction("Index", "Home");
            statut = "Actif";
        }
        else if (!EstEmployeAutorise())
        {
            return Forbid();
        }

        var vm = await _compteService.GetPagedAsync(recherche, agenceId, typeCompteId, statut, page, taille, clientId);
        return View(vm);
    }

    // GET: /Comptes/Details/5
    public async Task<IActionResult> Details(int id) {
        int? clientId = null;
        if (User.IsInRole("client"))
        {
            clientId = await GetClientIdAsync();
            if (!clientId.HasValue || !await AUnCompteActifAsync(clientId.Value))
                return RedirectToAction("Index", "Home");
            if (!await _context.Comptes.AnyAsync(c => c.Id == id && c.ClientId == clientId.Value && c.Statut == "Actif"))
                return Forbid();
        }
        else if (!EstEmployeAutorise()) return Forbid();

        var vm = await _compteService.GetDetailsAsync(id, clientId);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // GET: /Comptes/Create?clientId=5
    public async Task<IActionResult> Create(int? clientId) {
        if (!EstEmployeAutorise()) return Forbid();
        await ChargerListesAsync(clientId);
        return View(new CompteCreateViewModel { ClientId = clientId ?? 0 });
    }

    // POST: /Comptes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompteCreateViewModel vm) {
        if (!EstEmployeAutorise()) return Forbid();
        if (!ModelState.IsValid)
        {
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        if (await _compteService.NumeroCompteExisteAsync(vm.NumeroCompte.Trim().ToUpperInvariant())) {
            ModelState.AddModelError(nameof(vm.NumeroCompte), "Ce numéro de compte est déjà utilisé.");
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        try {
            await _compteService.CreateAsync(vm);
            TempData["Success"] = "Compte ouvert avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) 
            when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062) {
            // 1062 est le code d'erreur standard de MariaDB/MySQL pour les doublons (Duplicate entry)
            ModelState.AddModelError(nameof(vm.NumeroCompte), "Ce numéro de compte est déjà utilisé.");
            await ChargerListesAsync(vm.ClientId, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }
    }

    // GET: /Comptes/Edit/5
    public async Task<IActionResult> Edit(int id) {
        if (!EstEmployeAutorise()) return Forbid();
        var compte = await _compteService.GetByIdAsync(id);
        if (compte is null) return NotFound();

        var client = await _clientService.GetByIdAsync(compte.ClientId);

        var vm = new CompteEditViewModel {
            Id = compte.Id,
            NumeroCompte = compte.NumeroCompte,
            TypeCompteId = compte.TypeCompteId,
            AgenceId = compte.AgenceId,
            Statut = compte.Statut,
            Solde = compte.Solde,
            Devise = compte.Devise,
            ClientNom = client is null ? null : $"{client.Nom} {client.Prenom}"
        };

        await ChargerListesAsync(compte.ClientId, vm.TypeCompteId, vm.AgenceId);
        return View(vm);
    }

    // POST: /Comptes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CompteEditViewModel vm) {
        if (!EstEmployeAutorise()) return Forbid();
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) {
            await ChargerListesAsync(null, vm.TypeCompteId, vm.AgenceId);
            return View(vm);
        }

        var ok = await _compteService.UpdateAsync(vm);
        if (!ok) return NotFound();

        TempData["Success"] = "Compte modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Comptes/Delete/5
    public async Task<IActionResult> Delete(int id) {
        if (!EstEmployeAutorise()) return Forbid();
        var vm = await _compteService.GetDetailsAsync(id);
        if (vm is null) return NotFound();
        return View(vm);
    }

    // POST: /Comptes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireMfa")] 
    public async Task<IActionResult> DeleteConfirmed(int id) {
        if (!EstEmployeAutorise()) return Forbid();
        try {
            var ok = await _compteService.DeleteAsync(id);
            if (!ok) return NotFound();

            TempData["Success"] = "Compte supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task ChargerListesAsync(int? clientId = null, int? typeCompteId = null, int? agenceId = null) {
        // Clients
        var clientsVm = await _clientService.GetPagedAsync(null, null, null, 1, int.MaxValue);
        ViewBag.Clients = new SelectList(
            clientsVm.Clients.Select(c => new {
                c.Id,
                Libelle = $"{c.NumeroClient} — {c.Nom} {c.Prenom}"
            }),
            "Id", "Libelle", clientId);

        // Agences
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue);
        ViewBag.Agences = new SelectList(
            agencesVm.Agences.Select(a => new {
                a.Id,
                Libelle = $"{a.Code} — {a.Ville}"
            }),
            "Id", "Libelle", agenceId);

        // Types de compte
        var typesVm = await _compteService.GetPagedAsync(null, null, null, null, 1, int.MaxValue);
        ViewBag.TypesCompte = new SelectList(
            typesVm.TypesCompte.Select(t => new {
                t.Id,
                Libelle = string.Equals(t.Code, "EPARGNE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.Libelle, "Épargne", StringComparison.OrdinalIgnoreCase)
                    ? "Épargne"
                    : string.Equals(t.Code, "COURANT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(t.Libelle, "Courant", StringComparison.OrdinalIgnoreCase)
                        ? "Courant"
                        : t.Libelle
            }),
            "Id", "Libelle", typeCompteId);
    }

    private bool EstEmployeAutorise() =>
        User.IsInRole("admin") || User.IsInRole("gestionnaire") || User.IsInRole("guichetier") || User.IsInRole("auditeur");

    private async Task<int?> GetClientIdAsync()
    {
        var userId = await _currentUser.GetUserIdAsync();
        return await _context.Users.Where(u => u.Id == userId).Select(u => u.ClientId).FirstOrDefaultAsync();
    }

    private Task<bool> AUnCompteActifAsync(int clientId) =>
        _context.Comptes.AsNoTracking().AnyAsync(c => c.ClientId == clientId && c.Statut == "Actif");
}
