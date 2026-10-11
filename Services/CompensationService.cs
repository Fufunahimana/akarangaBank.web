using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class CompensationService : ICompensationService
{
    private readonly AkarangaBankContext _context;

    public CompensationService(AkarangaBankContext context) => _context = context;

    public async Task<CompensationAgence> CalculerCompensationAsync(int agenceId, DateTime date)
    {
        var agenceExiste = await _context.Agences.AnyAsync(a => a.Id == agenceId);
        if (!agenceExiste)
            throw new InvalidOperationException($"Agence {agenceId} introuvable.");

        var debut = date.Date;
        var (debutUtc, finUtc) = BankTime.GetUtcDayRange(debut);

        // A settled daily batch must remain immutable. Recalculating it could
        // reset a settled status and allow the same net position to be paid twice.
        var compensationReglee = await _context.CompensationsAgences
            .AnyAsync(c => c.DateCompensation >= debut && c.DateCompensation < debut.AddDays(1)
                && c.Statut == "Regle");
        if (compensationReglee)
            throw new InvalidOperationException("La compensation de cette journée est déjà réglée et ne peut plus être recalculée.");

        var virements = await _context.VirementsInterAgences.AsNoTracking()
            .Where(v => v.DateCompensation >= debutUtc && v.DateCompensation < finUtc && v.Statut == "EnCompensation")
            .ToListAsync();

        // Create or refresh both sides of the settlement batch, so the daily
        // control can compare all agency debits and credits after one calculation.
        var agences = virements.SelectMany(v => new[] { v.AgenceSourceId, v.AgenceDestinationId })
            .Append(agenceId)
            .Distinct()
            .ToArray();
        var fin = debut.AddDays(1);
        var resultatAgence = default(CompensationAgence);

        foreach (var id in agences)
        {
            var agence = await _context.Agences.FirstAsync(a => a.Id == id);
            var compteCompensation = await _context.Comptes
                .FirstOrDefaultAsync(c => c.AgenceId == id && c.NumeroCompte == $"9000-{id:D4}");

            var totalCredits = virements.Where(v => v.AgenceSourceId == id).Sum(v => v.Montant);
            var totalDebits = virements.Where(v => v.AgenceDestinationId == id).Sum(v => v.Montant);
            var compensation = await _context.CompensationsAgences
                .FirstOrDefaultAsync(c => c.AgenceId == id && c.DateCompensation >= debut && c.DateCompensation < fin);

            if (compensation is null)
            {
                compensation = new CompensationAgence
                {
                    Reference = $"COMP-{debut:yyyyMMdd}-{id:D4}",
                    AgenceId = id,
                    DateCompensation = debut,
                    CodeCompteGeneral = $"COMPENSATION:{agence.Code}"
                };
                _context.CompensationsAgences.Add(compensation);
            }

            compensation.CompteCompensationId = compteCompensation?.Id;
            compensation.TotalDebits = totalDebits;
            compensation.TotalCredits = totalCredits;
            compensation.Solde = totalCredits - totalDebits;
            compensation.Statut = compensation.Solde == 0m ? "Equilibre" : "EnCours";
            resultatAgence = id == agenceId ? compensation : resultatAgence;
        }

        await _context.SaveChangesAsync();
        return resultatAgence!;
    }

    public async Task<bool> ReglerCompensationAsync(int compensationId, int valideParId)
    {
        var compensation = await _context.CompensationsAgences
            .Include(c => c.CompteCompensation)
            .FirstOrDefaultAsync(c => c.Id == compensationId);
        if (compensation is null)
            return false;
        if (compensation.Statut == "Regle")
            return true;
        if (!compensation.CompteCompensationId.HasValue || compensation.CompteCompensation is null)
            throw new InvalidOperationException("Configurez un compte de compensation actif pour cette agence avant le règlement.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (compensation.Solde != 0m)
            {
                var montant = Math.Abs(compensation.Solde);
                var operation = new Operation
                {
                    Reference = $"REG-{compensation.Reference}",
                    TypeOperation = "ReglementCompensation",
                    CompteId = compensation.CompteCompensationId.Value,
                    UtilisateurId = valideParId,
                    AgenceId = compensation.AgenceId,
                    Montant = montant,
                    Devise = compensation.CompteCompensation.Devise,
                    Statut = "Valide",
                    DateOperation = DateTime.UtcNow,
                    DateValidation = DateTime.UtcNow,
                    Commentaire = $"Règlement de la compensation {compensation.Reference}"
                };
                _context.Operations.Add(operation);
                await _context.SaveChangesAsync();

                var compEstCreditrice = compensation.Solde > 0m;
                _context.Ecritures.AddRange(
                    new Ecriture
                    {
                        OperationId = operation.Id,
                        CompteId = compensation.CompteCompensationId,
                        CodeCompteGeneral = compensation.CodeCompteGeneral,
                        Sens = compEstCreditrice ? "Debit" : "Credit",
                        Montant = montant,
                        Devise = operation.Devise,
                        DateEcriture = DateTime.UtcNow,
                        Libelle = $"Règlement de {compensation.Reference}"
                    },
                    new Ecriture
                    {
                        OperationId = operation.Id,
                        CompteId = null,
                        CodeCompteGeneral = "REGLEMENT:BRB",
                        Sens = compEstCreditrice ? "Credit" : "Debit",
                        Montant = montant,
                        Devise = operation.Devise,
                        DateEcriture = DateTime.UtcNow,
                        Libelle = $"Contrepartie BRB de {compensation.Reference}"
                    });

                compensation.CompteCompensation.Solde += compEstCreditrice ? -montant : montant;
                compensation.CompteCompensation.DateModification = DateTime.UtcNow;
            }

            compensation.Statut = "Regle";
            compensation.DateReglement = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<CompensationAgence>> GetCompensationsAsync(int? agenceId, DateTime? date)
    {
        var query = _context.CompensationsAgences.AsNoTracking()
            .Include(c => c.Agence)
            .Include(c => c.CompteCompensation)
            .AsQueryable();
        if (agenceId.HasValue)
            query = query.Where(c => c.AgenceId == agenceId.Value);
        if (date.HasValue)
        {
            var debut = date.Value.Date;
            var fin = debut.AddDays(1);
            query = query.Where(c => c.DateCompensation >= debut && c.DateCompensation < fin);
        }
        return await query.OrderByDescending(c => c.DateCompensation).ThenBy(c => c.Agence.Code).ToListAsync();
    }

    public async Task<bool> VerifierEquilibreCompensationsAsync(DateTime date)
    {
        var debut = date.Date;
        var fin = debut.AddDays(1);
        var (debutUtc, finUtc) = BankTime.GetUtcDayRange(debut);
        var existeVirementACompense = await _context.VirementsInterAgences.AsNoTracking()
            .AnyAsync(v => v.DateCompensation >= debutUtc && v.DateCompensation < finUtc
                && v.Statut == "EnCompensation");
        if (!existeVirementACompense)
            return false;

        var compensations = await _context.CompensationsAgences.AsNoTracking()
            .Where(c => c.DateCompensation >= debut && c.DateCompensation < fin)
            .ToListAsync();
        return compensations.Count > 0
            && compensations.Sum(c => c.TotalDebits) == compensations.Sum(c => c.TotalCredits);
    }
}
