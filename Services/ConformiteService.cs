using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AkarangaBank.Web.Services;

public class ConformiteService : IConformiteService
{
    private readonly AkarangaBankContext _context;
    private readonly ISeuilAMLService? _seuils;
    private const decimal SeuilVigilanceRenforcee = 5_000_000m;
    private const decimal SeuilDeclarationEspece = 10_000_000m;

    public ConformiteService(AkarangaBankContext context, ISeuilAMLService? seuils = null)
    {
        _context = context;
        _seuils = seuils;
    }

    public async Task<decimal> GetSeuilAMLAsync()
    {
        if (_seuils is not null)
        {
            var configure = await _seuils.GetSeuilVigilanceAsync();
            return configure > 0 ? Math.Min(configure, SeuilVigilanceRenforcee) : SeuilVigilanceRenforcee;
        }

        var parametre = await _context.Parametres.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Cle == "SEUIL_AML");
        var valeur = parametre?.Valeur?.Trim()
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty)
            .Replace("\u202F", string.Empty);

        if (!decimal.TryParse(valeur, NumberStyles.Number, CultureInfo.InvariantCulture, out var seuil) || seuil <= 0)
            return SeuilVigilanceRenforcee;

        // Keep the 5,000,000 BIF vigilance rule active even if the database
        // contains a higher SEUIL_AML value. A lower value remains stricter.
        return Math.Min(seuil, SeuilVigilanceRenforcee);
    }

    public async Task<ResultatConformite> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation)
    {
        var compte = await _context.Comptes.Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == compteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        var seuilAML = await GetSeuilAMLAsync();
        var resultat = new ResultatConformite { CompteId = compteId, Montant = montant, DateAnalyse = DateTime.UtcNow };
        var seuilDeclaration = _seuils is null
            ? SeuilDeclarationEspece
            : await _seuils.GetSeuilDeclarationAsync(typeOperation);
        var seuilBlocage = _seuils is null ? 50_000_000m : await _seuils.GetSeuilBlocageAsync();

        if ((typeOperation is "Depot" or "Retrait") && montant >= seuilDeclaration)
        {
            resultat.NecessiteDeclaration = true;
            resultat.Motif = $"Transaction en espèces ≥ {seuilDeclaration:N0} BIF. Déclaration obligatoire à la CNRF.";
            resultat.Niveau = NiveauAlerte.Eleve;
        }

        if (montant >= seuilAML)
        {
            resultat.NecessiteVigilanceRenforcee = true;
            resultat.Motif ??= $"Montant ≥ {seuilAML:N0} BIF (seuil AML). Vérification de l'origine des fonds requise.";
            if (!resultat.NecessiteDeclaration)
                resultat.Niveau = NiveauAlerte.Moyen;
        }

        if (montant >= seuilBlocage)
        {
            resultat.EstBloquee = true;
            resultat.Motif = $"Montant supérieur ou égal au seuil de blocage ({seuilBlocage:N0} BIF).";
            resultat.Niveau = NiveauAlerte.Critique;
        }

        if ((typeOperation is "Retrait" or "Virement") && compte.Solde < montant)
        {
            resultat.EstBloquee = true;
            resultat.Motif = "Solde insuffisant.";
            resultat.Niveau = NiveauAlerte.Critique;
        }

        if (compte.Statut != "Actif")
        {
            resultat.EstBloquee = true;
            resultat.Motif = $"Compte {compte.Statut}. Opération impossible.";
            resultat.Niveau = NiveauAlerte.Critique;
        }

        return resultat;
    }

    public async Task<bool> DetecterFractionnementAsync(int compteId, decimal montant, DateTime date)
    {
        var seuil = await GetSeuilAMLAsync();
        var debut = date.AddHours(-24);
        var clientId = await _context.Comptes.AsNoTracking()
            .Where(c => c.Id == compteId)
            .Select(c => (int?)c.ClientId)
            .FirstOrDefaultAsync();
        if (!clientId.HasValue)
            return false;

        var alerteDejaCreee = await _context.AlertesAML.AnyAsync(a =>
            a.ClientId == clientId.Value && a.TypeAlerte == "Fractionnement" && a.DateAlerte >= debut && a.DateAlerte <= date);
        if (alerteDejaCreee)
            return false;

        var cumul = await _context.Ecritures
            .Where(e => e.CompteId == compteId
                && e.Sens == "Credit"
                && e.Operation.TypeOperation == "Depot"
                && e.DateEcriture >= debut
                && e.DateEcriture <= date
                && e.Montant < seuil)
            .SumAsync(e => (decimal?)e.Montant) ?? 0m;
        return cumul + montant >= seuil && montant < seuil;
    }

    public async Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId)
    {
        var existe = await _context.Clients.AnyAsync(c => c.Id == clientId);
        if (!existe)
            return NiveauRisqueClient.Inconnu;

        var score = 0;
        if (await _context.Comptes.CountAsync(c => c.ClientId == clientId) > 3)
            score += 2;
        if (await _context.Ecritures.AnyAsync(e => e.Compte != null && e.Compte.ClientId == clientId && e.Montant >= SeuilVigilanceRenforcee))
            score += 3;
        if (await _context.AlertesAML.AnyAsync(a => a.ClientId == clientId))
            score += 5;

        return score >= 6 ? NiveauRisqueClient.Eleve
            : score >= 3 ? NiveauRisqueClient.Moyen
            : NiveauRisqueClient.Faible;
    }
}
