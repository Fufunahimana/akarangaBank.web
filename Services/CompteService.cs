using Microsoft.EntityFrameworkCore; 
using AkarangaBank.Web.Data; 
using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 
namespace AkarangaBank.Web.Services; 
 
public class CompteService : ICompteService { 
    private readonly AkarangaBankContext _context; 
 
    public CompteService(AkarangaBankContext context) { 
        _context = context; 
    } 
 
    public async Task<CompteListViewModel> GetPagedAsync( 
        string? recherche, int? agenceId, int? typeCompteId, string? statut, int page, int taille) { 
        var query = _context.Comptes 
            .AsNoTracking() 
            .Include(c => c.Client) 
            .Include(c => c.TypeCompte) 
            .Include(c => c.Agence) 
            .AsQueryable(); 
 
        if (!string.IsNullOrWhiteSpace(recherche)) { 
            var r = recherche.Trim(); 
            query = query.Where(c => 
                c.NumeroCompte.Contains(r) || 
                c.Client.Nom.Contains(r) || 
                c.Client.Prenom.Contains(r) || 
                c.Client.NumeroClient.Contains(r)); 
        } 
 
        if (agenceId.HasValue) 
            query = query.Where(c => c.AgenceId == agenceId.Value); 
 
        if (typeCompteId.HasValue) 
            query = query.Where(c => c.TypeCompteId == typeCompteId.Value); 
 
        if (!string.IsNullOrWhiteSpace(statut)) 
            query = query.Where(c => c.Statut == statut); 
 
        var total = await query.CountAsync(); 
 
        var comptes = await query 
            .OrderBy(c => c.NumeroCompte) 
            .Skip((page - 1) * taille) 
            .Take(taille) 
            .ToListAsync(); 
 
        var agences = await _context.Agences 
            .AsNoTracking() 
            .OrderBy(a => a.Ville) 
            .ToListAsync(); 
 
        var types = await _context.TypesCompte 
            .AsNoTracking() 
            .OrderBy(t => t.Code) 
            .ToListAsync(); 
 
        return new CompteListViewModel { 
            Comptes = comptes, 
            Recherche = recherche, 
            AgenceId = agenceId, 
            TypeCompteId = typeCompteId, 
            Statut = statut, 
            Page = page, 
            TaillePage = taille, 
            Total = total, 
            Agences = agences, 
            TypesCompte = types 
        }; 
    } 
 
    public async Task<CompteDetailsViewModel?> GetDetailsAsync(int id){ 
        var compte = await _context.Comptes 
            .AsNoTracking() 
            .Include(c => c.Client) 
            .Include(c => c.TypeCompte) 
            .Include(c => c.Agence) 
            .FirstOrDefaultAsync(c => c.Id == id); 
 
        if (compte is null) return null; 
 
        var ecritures = await _context.Ecritures 
            .AsNoTracking() 
            .Include(e => e.Operation) 
            .Where(e => e.CompteId == id) 
            .OrderByDescending(e => e.DateEcriture) 
            .Take(50) 
            .ToListAsync(); 
 
        return new CompteDetailsViewModel { 
            Compte = compte, 
            Ecritures = ecritures 
        }; 
    } 
 
    public async Task<Compte?> GetByIdAsync(int id) { 
        return await _context.Comptes 
            .AsNoTracking() 
            .FirstOrDefaultAsync(c => c.Id == id); 
    } 
 
    public async Task<Compte> CreateAsync(CompteCreateViewModel vm){ 
        var compte = new Compte 
        { 
            NumeroCompte = vm.NumeroCompte.Trim().ToUpperInvariant(), 
            ClientId = vm.ClientId, 
            TypeCompteId = vm.TypeCompteId, 
            AgenceId = vm.AgenceId, 
            Solde = vm.SoldeInitial, 
            Devise = "BIF", 
            Statut = "Actif", 
            DateOuverture = DateTime.UtcNow, 
            DateCreation = DateTime.UtcNow 
        }; 
 
        _context.Comptes.Add(compte); 
        await _context.SaveChangesAsync(); 
        return compte; 
    } 
    public async Task<bool> UpdateAsync(CompteEditViewModel vm) { 
        var compte = await _context.Comptes.FindAsync(vm.Id); 
        if (compte is null) return false; 
 
        compte.NumeroCompte = vm.NumeroCompte.Trim().ToUpperInvariant(); 
        compte.TypeCompteId = vm.TypeCompteId; 
        compte.AgenceId = vm.AgenceId; 
        compte.Statut = vm.Statut; 
        compte.DateModification = DateTime.UtcNow; 
 
        if (vm.Statut == "Cloture") 
            compte.DateFermeture = DateTime.UtcNow; 
 
        await _context.SaveChangesAsync(); 
        return true; 
    } 
 
    public async Task<bool> DeleteAsync(int id){ 
        var compte = await _context.Comptes 
            .Include(c => c.Ecritures) 
            .FirstOrDefaultAsync(c => c.Id == id); 
 
        if (compte is null) return false; 
 
        if (compte.Ecritures.Any()) 
            throw new InvalidOperationException("Impossible de supprimer ce compte : des écritures comptables y sont rattachées.");
        _context.Comptes.Remove(compte); 
        await _context.SaveChangesAsync(); 
        return true; 
    } 
 
    public async Task<bool> NumeroCompteExisteAsync(string numeroCompte, int? excludeId = null) { 
        var query = _context.Comptes.Where(c => c.NumeroCompte == numeroCompte); 
        if (excludeId.HasValue) 
            query = query.Where(c => c.Id != excludeId.Value); 
        return await query.AnyAsync(); 
    } 
} 
