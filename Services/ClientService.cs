using Microsoft.EntityFrameworkCore;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;

namespace AkarangaBank.Web.Services;

public class ClientService : IClientService 
{
    private readonly AkarangaBankContext _context;

    public ClientService(AkarangaBankContext context) 
    {
        _context = context;
    }
 
    public async Task<ClientListViewModel> GetPagedAsync( 
     string? recherche, int? agenceId, string? statut,int page,int taille) 
    { 
        var query = _context.Clients 
            .AsNoTracking() 
            .Include(c => c.Agence) 
            .AsQueryable(); 
 
        if (!string.IsNullOrWhiteSpace(recherche)){ 
            var r = recherche.Trim(); 
            query = query.Where(c => 
                c.NumeroClient.Contains(r) ||  
                c.Nom.Contains(r) || 
                c.Prenom.Contains(r) || 
                c.NumeroCNI.Contains(r) || 
                (c.Email != null && c.Email.Contains(r))); 
        } 
 
        if (agenceId.HasValue) 
            query = query.Where(c => c.AgenceId == agenceId.Value); 
 
        if (!string.IsNullOrWhiteSpace(statut)) 
            query = query.Where(c => c.Statut == statut); 
 
        var total = await query.CountAsync(); 
 
        var clients = await query 
            .OrderBy(c => c.Nom) 
            .ThenBy(c => c.Prenom) 
            .Skip((page - 1) * taille) 
            .Take(taille) 
            .ToListAsync(); 
 
        // Liste des agences pour le filtre 
        var agences = await _context.Agences 
            .AsNoTracking() 
            .OrderByDescending(a => a.EstSiege) 
            .ThenBy(a => a.Ville) 
            .ToListAsync(); 
 
        return new ClientListViewModel 
        { 
            Clients = clients, 
            Recherche = recherche, 
            AgenceId = agenceId, 
            Statut = statut, 
            Page = page, 
            TaillePage = taille, 
            Total = total, 
            Agences = agences 
        }; 
    } 
    public async Task<ClientDetailsViewModel?> GetDetailsAsync(int id) 
    { 
        var client = await _context.Clients 
            .AsNoTracking() 
            .Include(c => c.Agence) 
            .FirstOrDefaultAsync(c => c.Id == id); 
 
        if (client is null) return null; 
 
        var comptes = await _context.Comptes 
            .AsNoTracking() 
            .Include(c => c.TypeCompte) 
            .Where(c => c.ClientId == id) 
            .OrderBy(c => c.NumeroCompte) 
            .ToListAsync(); 
 
        var kyc = await _context.VerificationsKYC 
            .AsNoTracking() 
            .Include(v => v.VerifiePar) 
            .Where(v => v.ClientId == id) 
            .OrderByDescending(v => v.DateVerification) 
            .ToListAsync(); 
 
        return new ClientDetailsViewModel 
        { 
            Client = client, 
            Comptes = comptes, 
            VerificationsKYC = kyc 
        }; 
    } 
 
    public async Task<Client?> GetByIdAsync(int id) 
    { 
        return await _context.Clients 
            .AsNoTracking() 
            .FirstOrDefaultAsync(c => c.Id == id); 
    } 
 
    public async Task<Client> CreateAsync(ClientCreateViewModel vm) 
    { 
        var client = new Client 
        { 
            NumeroClient = vm.NumeroClient.Trim().ToUpperInvariant(),  
            Nom = vm.Nom.Trim(), 
            Prenom = vm.Prenom.Trim(), 
            DateNaissance = vm.DateNaissance, 
            NumeroCNI = vm.NumeroCNI.Trim().ToUpperInvariant(), 
            Telephone = vm.Telephone?.Trim(), 
            Email = vm.Email?.Trim(), 
            Adresse = vm.Adresse?.Trim(), 
            AgenceId = vm.AgenceId, 
            Statut = "Actif", 
            DateCreation = DateTime.UtcNow 
        }; 
 
        _context.Clients.Add(client); 
        await _context.SaveChangesAsync(); 
        return client; 
    } 
 
    public async Task<bool> UpdateAsync(ClientEditViewModel vm) 
    { 
        var client = await _context.Clients.FindAsync(vm.Id); 
        if (client is null) return false; 
 
        client.NumeroClient = vm.NumeroClient.Trim().ToUpperInvariant(); 
        client.Nom = vm.Nom.Trim(); 
        client.Prenom = vm.Prenom.Trim(); 
        client.DateNaissance = vm.DateNaissance; 
        client.NumeroCNI = vm.NumeroCNI.Trim().ToUpperInvariant(); 
        client.Telephone = vm.Telephone?.Trim(); 
        client.Email = vm.Email?.Trim(); 
        client.Adresse = vm.Adresse?.Trim(); 
        client.AgenceId = vm.AgenceId; 
        client.Statut = vm.Statut; 
        client.DateModification = DateTime.UtcNow; 
 
        await _context.SaveChangesAsync(); 
        return true; 
    } 
 
    public async Task<bool> DeleteAsync(int id) 
    { 
        var client = await _context.Clients 
            .Include(c => c.Comptes) 
            .Include(c => c.VerificationsKYC) 
 
            .FirstOrDefaultAsync(c => c.Id == id); 
 
        if (client is null) return false; 
 
        if (client.Comptes.Any()) 
            throw new InvalidOperationException( 
              "Impossible de supprimer ce client : il possède des comptes bancaires."); 
 
        // Suppression des vérifications KYC rattachées 
        if (client.VerificationsKYC.Any()) 
            _context.VerificationsKYC.RemoveRange(client.VerificationsKYC); 
 
        _context.Clients.Remove(client); 
        await _context.SaveChangesAsync(); 
        return true; 
    } 
 
    public async Task<bool> NumeroClientExisteAsync(string numeroClient, int? excludeId = null) 
    { 
        var query = _context.Clients.Where(c => c.NumeroClient == numeroClient); 
        if (excludeId.HasValue) 
            query = query.Where(c => c.Id != excludeId.Value); 
        return await query.AnyAsync(); 
    } 
 
    public async Task<bool> NumeroCNIExisteAsync(string numeroCNI,int? excludeId = null) 
    { 
        var query = _context.Clients.Where(c => c.NumeroCNI == numeroCNI); 
        if (excludeId.HasValue) 
            query = query.Where(c => c.Id != excludeId.Value); 
        return await query.AnyAsync(); 
    } 
}