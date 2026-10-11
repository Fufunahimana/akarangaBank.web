using Microsoft.EntityFrameworkCore; 
using AkarangaBank.Web.Data; 
using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Services; 
 
// AkarangaBank — Service Agence 
// Akaranga (kirundi) : culture, bon caractère, bien éduqué. 
// Ce service encapsule la logique métier des agences, séparée du contrôleur. 
public class AgenceService : IAgenceService 
{ 
    private readonly AkarangaBankContext _context; 
 
    public AgenceService(AkarangaBankContext context) 
    { 
        _context = context; 
    } 
 
    // ------------------------------------------------------------- 
    // Liste paginée avec recherche 
    // ------------------------------------------------------------ 
    public async Task<AgenceListViewModel> GetPagedAsync(string? recherche, int page, int taille) 
    { 
        var query = _context.Agences.AsNoTracking().AsQueryable(); 
 
        // Recherche par code, nom ou ville 
        if (!string.IsNullOrWhiteSpace(recherche)) 
        { 
            var r = recherche.Trim(); 
            query = query.Where(a => 
                a.Code.Contains(r) || 
                a.Nom.Contains(r) || 
                a.Ville.Contains(r)); 
        } 
        var total = await query.CountAsync(); 
 
        var agences = await query 
            .OrderByDescending(a => a.EstSiege)  // Le siège en premier 
            .ThenBy(a => a.Ville) 
            .Skip((page - 1) * taille) 
            .Take(taille) 
            .ToListAsync(); 
 
        return new AgenceListViewModel 
        { 
            Agences = agences, 
            Recherche = recherche, 
            Page = page, 
            TaillePage = taille, 
            Total = total 
        }; 
    } 
 
    // ------------------------------------------------------------ 
    // Détails avec compteurs (clients, comptes, utilisateurs) 
    // ------------------------------------------------------------ 
    public async Task<AgenceDetailsViewModel?> GetDetailsAsync(int id) 
    { 
        var agence = await _context.Agences
            .AsNoTracking() 
            .FirstOrDefaultAsync(a => a.Id == id); 
 
        if (agence is null) return null; 
 
        return new AgenceDetailsViewModel 
        { 
            Agence = agence, 
            NombreClients = await _context.Clients 
                .CountAsync(c => c.AgenceId == id),NombreComptes = await _context.Comptes 
                .CountAsync(c => c.AgenceId == id), 
            NombreUtilisateurs = await _context.Utilisateurs.CountAsync(u => u.AgenceId == id) 
        }; 
    } 

    // ------------------------------------------------------------- 
    // Récupération simple 
    // ------------------------------------------------------------- 
    public async Task<Agence?> GetByIdAsync(int id) 
    { 
        return await _context.Agences 
            .AsNoTracking() 
            .FirstOrDefaultAsync(a => a.Id == id); 
    } 
    // ------------------------------------------------------------- 
    // Création 
    // ------------------------------------------------------------- 
    public async Task<Agence> CreateAsync(AgenceCreateViewModel vm) 
    { 
        var agence = new Agence 
        { 
            Code = vm.Code.Trim().ToUpperInvariant(), 
            Nom = vm.Nom.Trim(), 
            Ville = vm.Ville.Trim(), 
            EstSiege = vm.EstSiege, 
            DateCreation = DateTime.UtcNow 
        }; 
 
        _context.Agences.Add(agence); 
        await _context.SaveChangesAsync(); 
        return agence; 
    } 
    // ------------------------------------------------------------- 
    // Modification 
    // ------------------------------------------------------------- 
    public async Task<bool> UpdateAsync(AgenceEditViewModel vm) { 
        var agence = await _context.Agences.FindAsync(vm.Id); 
        if (agence is null) return false; 
 
        agence.Code = vm.Code.Trim().ToUpperInvariant(); 
        agence.Nom = vm.Nom.Trim(); 
        agence.Ville = vm.Ville.Trim(); 
        agence.EstSiege = vm.EstSiege; 
        agence.DateModification = DateTime.UtcNow; 
 
        await _context.SaveChangesAsync(); 
        return true; 
    }  
    // ------------------------------------------------------------- 
    // Suppression — refusée si l'agence a des dépendances 
    // ------------------------------------------------------------- 
    public async Task<bool> DeleteAsync(int id) { 
        var agence = await _context.Agences.FindAsync(id); 
        if (agence is null) return false; 
 
        // Vérification d'intégrité référentielle 
        if (!await APeuEtreSupprimeeAsync(id)) 
            throw new InvalidOperationException( 
                "Impossible de supprimer cette agence : des clients, comptes " + 
                                        "ou utilisateurs y sont rattachés."); 
 
        _context.Agences.Remove(agence); 
        await _context.SaveChangesAsync(); 
        return true; 
    } 
 
    // ------------------------------------------------------------- 
    // Vérifications métier 
    // ------------------------------------------------------------- 
    public async Task<bool> CodeExisteAsync(string code, int? excludeId = null){ 
        var query = _context.Agences.Where(a => a.Code == code); 
        if (excludeId.HasValue) 
            query = query.Where(a => a.Id != excludeId.Value); 
        return await query.AnyAsync(); 
    } 
 
    public async Task<bool> APeuEtreSupprimeeAsync(int id){ 
       var aDesClients = await _context.Clients.AnyAsync(c => c.AgenceId == id); 
        var aDesComptes = await _context.Comptes.AnyAsync(c => c.AgenceId == id); 
        var aDesUtilisateurs = await _context.Utilisateurs.AnyAsync(u => u.AgenceId == id); 
        return !aDesClients && !aDesComptes && !aDesUtilisateurs; 
    } 
}