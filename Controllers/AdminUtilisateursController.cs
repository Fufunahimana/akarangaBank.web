using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.Rendering; 
using AkarangaBank.Web.Services; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Controllers; 
 
[Authorize(Policy = "AdminOnly")] 
public class AdminUtilisateursController : Controller { 
    private readonly IUtilisateurService _utilisateurService; 
    private readonly IAgenceService _agenceService; 
 
    public AdminUtilisateursController( 
        IUtilisateurService utilisateurService, 
        IAgenceService agenceService) 
    { 
        _utilisateurService = utilisateurService; 
        _agenceService = agenceService; 
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
} 
