using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MySqlConnector; // Namespace pour les exceptions MariaDB/MySQL
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Controllers;

public class ClientsController : Controller 
{ 
    private readonly IClientService _clientService; 
    private readonly IAgenceService _agenceService; 

    public ClientsController(IClientService clientService, IAgenceService agenceService) 
    { 
        _clientService = clientService; 
        _agenceService = agenceService; 
    } 

    // GET: /Clients 
    public async Task<IActionResult> Index(string? recherche, int? agenceId, string? statut, int page = 1) 
    { 
        const int taille = 10; 
        var vm = await _clientService.GetPagedAsync(recherche, agenceId, statut, page, taille); 
        return View(vm); 
    } 

    // GET: /Clients/Details/5 
    public async Task<IActionResult> Details(int id) 
    { 
        var vm = await _clientService.GetDetailsAsync(id); 
        if (vm is null) return NotFound(); 
        return View(vm); 
    } 

    // GET: /Clients/Create 
    public async Task<IActionResult> Create() 
    { 
        await ChargerAgencesAsync(); 
        return View(new ClientCreateViewModel()); 
    } 

    // POST: /Clients/Create 
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Create(ClientCreateViewModel vm) 
    { 
        if (!ModelState.IsValid) 
        { 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        if (await _clientService.NumeroClientExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant())) { 
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro de client est déjà utilisé."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        if (await _clientService.NumeroCNIExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant())) 
        { 
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce numéro de CNI est déjà utilisé."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        try { 
            await _clientService.CreateAsync(vm); 
            TempData["Success"] = "Client créé avec succès."; 
            return RedirectToAction(nameof(Index)); 
        } 
        catch (DbUpdateException ex) 
            when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062) 
        { 
            // 1062 correspond à une entrée en double sous MariaDB / MySQL
            ModelState.AddModelError("", "Un client avec ce numéro ou ce CNI existe déjà."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 
    } 

    // GET: /Clients/Edit/5 
    public async Task<IActionResult> Edit(int id) { 
        var client = await _clientService.GetByIdAsync(id); 
        if (client is null) return NotFound(); 

        var vm = new ClientEditViewModel { 
            Id = client.Id, 
            NumeroClient = client.NumeroClient, 
            Nom = client.Nom, 
            Prenom = client.Prenom, 
            DateNaissance = client.DateNaissance, 
            NumeroCNI = client.NumeroCNI, 
            Telephone = client.Telephone, 
            Email = client.Email, 
            Adresse = client.Adresse, 
            AgenceId = client.AgenceId, 
            Statut = client.Statut 
        }; 

        await ChargerAgencesAsync(vm.AgenceId); 
        return View(vm); 
    } 

    // POST: /Clients/Edit/5 
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Edit(int id, ClientEditViewModel vm) { 
        if (id != vm.Id) return BadRequest(); 
        if (!ModelState.IsValid) { 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        if (await _clientService.NumeroClientExisteAsync(vm.NumeroClient.Trim().ToUpperInvariant(), vm.Id)) { 
            ModelState.AddModelError(nameof(vm.NumeroClient), "Ce numéro de client est déjà utilisé."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        if (await _clientService.NumeroCNIExisteAsync(vm.NumeroCNI.Trim().ToUpperInvariant(), vm.Id)) { 
            ModelState.AddModelError(nameof(vm.NumeroCNI), "Ce numéro de CNI est déjà utilisé."); 
            await ChargerAgencesAsync(vm.AgenceId); 
            return View(vm); 
        } 

        var ok = await _clientService.UpdateAsync(vm); 
        if (!ok) return NotFound(); 

        TempData["Success"] = "Client modifié avec succès."; 
        return RedirectToAction(nameof(Index)); 
    } 

    // GET: /Clients/Delete/5 
    public async Task<IActionResult> Delete(int id) { 
        var vm = await _clientService.GetDetailsAsync(id); 
        if (vm is null) return NotFound(); 
        return View(vm); 
    } 

    // POST: /Clients/Delete/5 
    [HttpPost, ActionName("Delete")] 
    [ValidateAntiForgeryToken] 
    [Authorize(Policy = "RequireMfa")] 
    public async Task<IActionResult> DeleteConfirmed(int id) { 
        try { 
            var ok = await _clientService.DeleteAsync(id); 
            if (!ok) return NotFound(); 

            TempData["Success"] = "Client supprimé avec succès."; 
            return RedirectToAction(nameof(Index)); 
        } 
        catch (InvalidOperationException ex) { 
            TempData["Error"] = ex.Message; 
            return RedirectToAction(nameof(Index)); 
        } 
    } 

    private async Task ChargerAgencesAsync(int? selectedId = null) { 
        var agencesVm = await _agenceService.GetPagedAsync(null, 1, int.MaxValue); 
        ViewBag.Agences = new SelectList( 
            agencesVm.Agences.Select(a => new 
            { 
                a.Id, 
                Libelle = $"{a.Code} — {a.Nom}" 
            }), 
            "Id", "Libelle", selectedId); 
    } 
}