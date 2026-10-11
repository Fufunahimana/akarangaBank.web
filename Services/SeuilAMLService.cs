using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class SeuilAMLService : ISeuilAMLService
{
    private readonly AkarangaBankContext _context;

    public SeuilAMLService(AkarangaBankContext context) => _context = context;

    public async Task<SeuilAML?> GetSeuilAsync(string code, string typeOperation)
    {
        var query = _context.SeuilsAML.AsNoTracking()
            .Where(s => s.Code == code && s.Actif);

        if (!string.Equals(typeOperation, "Toutes", StringComparison.OrdinalIgnoreCase))
            query = query.Where(s => s.TypeOperation == typeOperation || s.TypeOperation == "Toutes");

        return await query
            .OrderByDescending(s => s.TypeOperation == typeOperation)
            .ThenBy(s => s.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SeuilAML>> GetAllAsync() => await _context.SeuilsAML
        .AsNoTracking()
        .OrderBy(s => s.Code)
        .ThenBy(s => s.TypeOperation)
        .ToListAsync();

    public async Task<bool> MettreAJourAsync(int seuilId, decimal nouveauMontant, int utilisateurId)
    {
        if (nouveauMontant <= 0)
            throw new ArgumentOutOfRangeException(nameof(nouveauMontant), "Le seuil doit être supérieur à zéro.");

        var seuil = await _context.SeuilsAML.FirstOrDefaultAsync(s => s.Id == seuilId);
        if (seuil is null)
            return false;

        var ancienMontant = seuil.Montant;
        seuil.Montant = nouveauMontant;
        seuil.DateModification = DateTime.UtcNow;
        _context.AuditLogs.Add(new AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = utilisateurId,
            Action = "ModificationSeuilAML",
            TableConcernee = "SeuilsAML",
            EnregistrementId = seuil.Id,
            ValeurAvant = ancienMontant.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ValeurApres = nouveauMontant.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Details = $"Seuil {seuil.Code}/{seuil.TypeOperation} modifié."
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetSeuilVigilanceAsync() =>
        (await GetSeuilAsync("VIGILANCE", "Toutes"))?.Montant ?? 5_000_000m;

    public async Task<decimal> GetSeuilDeclarationAsync(string typeOperation = "Toutes") =>
        (await GetSeuilAsync("DECLARATION", typeOperation))?.Montant ?? 10_000_000m;

    public async Task<decimal> GetSeuilBlocageAsync() =>
        (await GetSeuilAsync("BLOCAGE", "Toutes"))?.Montant ?? 50_000_000m;
}
