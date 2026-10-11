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
public class CartesController : Controller
{
    private readonly AkarangaBankContext _context;
    private readonly ICarteService _cartes;
    private readonly ICurrentUserService _currentUser;

    public CartesController(AkarangaBankContext context, ICarteService cartes, ICurrentUserService currentUser)
    {
        _context = context;
        _cartes = cartes;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var query = _context.Cartes.AsNoTracking().Include(c => c.Compte).Include(c => c.Client).AsQueryable();
        if (User.IsInRole("client"))
        {
            var clientId = await GetClientIdAsync();
            query = clientId.HasValue ? query.Where(c => c.ClientId == clientId.Value) : query.Where(c => false);
        }
        else if (!User.IsInRole("admin") && !User.IsInRole("gestionnaire") && !User.IsInRole("guichetier") && !User.IsInRole("auditeur"))
        {
            return Forbid();
        }
        return View(await query.OrderByDescending(c => c.DateCreation).ToListAsync());
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create()
    {
        await ChargerComptesAsync();
        return View(new CarteEmissionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CarteEmissionViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
        try
        {
            var carte = await _cartes.EmettreAsync(vm);
            TempData["Success"] = $"Carte {carte.NumeroCarteMasque} émise avec succès.";
            return RedirectToAction(nameof(Details), new { id = carte.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ChargerComptesAsync(vm.CompteId);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var vm = await ChargerDetailsAsync(id);
        if (vm is null) return NotFound();
        if (!await ALeDroitSurCarteAsync(vm.Carte, operationFinanciere: false)) return Forbid();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Historique(int id)
    {
        var vm = await ChargerDetailsAsync(id);
        if (vm is null) return NotFound();
        if (!await ALeDroitSurCarteAsync(vm.Carte, operationFinanciere: false)) return Forbid();
        return View("Details", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Bloquer(CarteBlocageViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid) throw new InvalidOperationException("Le motif du blocage est obligatoire.");
            var carte = await _cartes.BloquerAsync(vm.CarteId, vm.Motif);
            TempData["Success"] = $"Carte mise en opposition ({carte.Statut}).";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = vm.CarteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Renouveler(int id)
    {
        try
        {
            var carte = await _cartes.RenouvelerAsync(id);
            TempData["Success"] = $"Nouvelle carte émise : {carte.NumeroCarteMasque}.";
            return RedirectToAction(nameof(Details), new { id = carte.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transaction(TransactionCarteViewModel vm)
    {
        if (!await _context.Cartes.AnyAsync(c => c.Id == vm.CarteId)) return NotFound();
        var carte = await _context.Cartes.AsNoTracking().FirstAsync(c => c.Id == vm.CarteId);
        if (!await ALeDroitSurCarteAsync(carte, operationFinanciere: true)) return Forbid();
        try
        {
            var resultat = await _cartes.AutoriserTransactionAsync(vm.CarteId, vm.Montant, vm.TypeTransaction, await _currentUser.GetUserIdAsync());
            if (resultat.Statut == "Autorisee")
                TempData["Success"] = $"Transaction autorisée et comptabilisée : {resultat.Montant:N2} BIF.";
            else
                TempData["Error"] = $"Transaction refusée : {resultat.MotifRefus}";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id = vm.CarteId });
    }

    private async Task<CarteDetailsViewModel?> ChargerDetailsAsync(int id)
    {
        var carte = await _context.Cartes.AsNoTracking().Include(c => c.Client).Include(c => c.Compte).ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (carte is null) return null;
        return new CarteDetailsViewModel { Carte = carte, Transactions = await _cartes.GetHistoriqueAsync(id) };
    }

    private async Task<bool> ALeDroitSurCarteAsync(Carte carte, bool operationFinanciere)
    {
        if (User.IsInRole("admin") || User.IsInRole("gestionnaire") || User.IsInRole("guichetier")) return true;
        if (User.IsInRole("auditeur")) return !operationFinanciere;
        if (!User.IsInRole("client")) return false;
        var clientId = await GetClientIdAsync();
        return clientId == carte.ClientId;
    }

    private async Task<int?> GetClientIdAsync()
    {
        var userId = await _currentUser.GetUserIdAsync();
        return await _context.Users.Where(u => u.Id == userId).Select(u => u.ClientId).FirstOrDefaultAsync();
    }

    private async Task ChargerComptesAsync(int? selectedId = null)
    {
        var comptes = await _context.Comptes.AsNoTracking().Include(c => c.Client)
            .Where(c => c.Statut == "Actif")
            .OrderBy(c => c.NumeroCompte)
            .Select(c => new { c.Id, Libelle = c.NumeroCompte + " — " + c.Client.Nom + " " + c.Client.Prenom })
            .ToListAsync();
        ViewBag.Comptes = new SelectList(comptes, "Id", "Libelle", selectedId);
        ViewBag.TypesCarte = new SelectList(new[] { "Visa", "MasterCard", "Locale" });
    }
}
