using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.Rendering; 
using AkarangaBank.Web.Services; 
using AkarangaBank.Web.ViewModels; 
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;
 
namespace AkarangaBank.Web.Controllers; 
 
[Authorize(Policy = "AdminOnly")] 
public class AdminUtilisateursController : Controller { 
    private readonly IUtilisateurService _utilisateurService; 
    private readonly IAgenceService _agenceService; 
    private readonly AkarangaBankContext _context;
 
    public AdminUtilisateursController( 
        IUtilisateurService utilisateurService, 
        IAgenceService agenceService,
        AkarangaBankContext context) 
    { 
        _utilisateurService = utilisateurService; 
        _agenceService = agenceService; 
        _context = context;
    } 
 
    public async Task<IActionResult> Index() 
    { 
        var users = await _utilisateurService.GetAllAsync(); 
        return View(users); 
    } 
 
    public async Task<IActionResult> Create() 
    { 
        await ChargerAgencesAsync(); 
        return View(new UtilisateurCreateViewModel()); 
    } 

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _utilisateurService.GetByIdAsync(id);
        if (user is null) return NotFound();
        await ChargerDonneesEditionAsync(user.ClientId, user.Role);
        return View(new UtilisateurEditViewModel
        {
            Id = user.Id,
            NomComplet = user.NomComplet,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            ClientId = user.ClientId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UtilisateurEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await ChargerDonneesEditionAsync(vm.ClientId, vm.Role);
            return View(vm);
        }

        var (success, error) = await _utilisateurService.UpdateProfileAsync(vm);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Impossible de modifier l'utilisateur.");
            await ChargerDonneesEditionAsync(vm.ClientId, vm.Role);
            return View(vm);
        }

        TempData["Success"] = "Le profil de l'utilisateur a été modifié.";
        return RedirectToAction(nameof(Index));
    }
 
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Create(UtilisateurCreateViewModel vm) 
    { 
        if (!ModelState.IsValid) 
        { 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        }  
 
        var (success, error) = await _utilisateurService.CreateAsync(vm); 
        if (!success) 
        { 
            ModelState.AddModelError("", error ?? "Erreur inconnue."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 
 
        TempData["Success"] = "Utilisateur créé avec succès."; 
        return RedirectToAction(nameof(Index)); 
    } 
 
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> ToggleActif(int id) 
    { 
        var (success, error) = await _utilisateurService.ToggleActifAsync(id);
        if (!success)
        {
            TempData["Error"] = error ?? "Impossible de modifier le statut.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Statut modifié.";
        return RedirectToAction(nameof(Index)); 
    } 
 
    private async Task ChargerAgencesAsync(int? selectedId = null) 
    { 
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue); 
        ViewBag.Agences = new SelectList( 
            agencesVm.Agences.Select(a => new { a.Id, Libelle = $"{a.Code} — {a.Ville}" }), "Id", "Libelle", selectedId); 
 
        ViewBag.Roles = new SelectList(new[] 
        { 
            "admin", "gestionnaire", "auditeur", "guichetier", "client" 
        }); 
    } 

    private async Task ChargerDonneesEditionAsync(int? clientId, string? role)
    {
        ViewBag.Roles = new SelectList(new[] { "admin", "gestionnaire", "auditeur", "guichetier", "client" }, role);
        ViewBag.Clients = new SelectList(
            await _context.Clients.AsNoTracking().OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
                .Select(c => new { c.Id, Libelle = $"{c.NumeroClient} — {c.Nom} {c.Prenom}" }).ToListAsync(),
            "Id", "Libelle", clientId);
    }
} 
