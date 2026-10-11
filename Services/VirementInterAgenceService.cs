using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class VirementInterAgenceService : IVirementInterAgenceService
{
    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;
    private readonly IDetectionAvanceeService _detection;
    private readonly IDeclarationService _declarations;

    public VirementInterAgenceService(
        AkarangaBankContext context,
        IConformiteService conformite,
        IDetectionAvanceeService detection,
        IDeclarationService declarations)
    {
        _context = context;
        _conformite = conformite;
        _detection = detection;
        _declarations = declarations;
    }

    public async Task<VirementInterAgence> InitierAsync(VirementInterAgenceViewModel vm, int utilisateurId)
    {
        if (vm.Montant <= 0)
            throw new InvalidOperationException("Le montant doit être supérieur à zéro.");
        if (vm.CompteSourceId == vm.CompteDestinationId)
            throw new InvalidOperationException("Les comptes source et destination doivent être différents.");

        var source = await _context.Comptes.Include(c => c.Agence)
            .FirstOrDefaultAsync(c => c.Id == vm.CompteSourceId)
            ?? throw new InvalidOperationException("Compte source introuvable.");
        var destination = await _context.Comptes.Include(c => c.Agence)
            .FirstOrDefaultAsync(c => c.Id == vm.CompteDestinationId)
            ?? throw new InvalidOperationException("Compte destination introuvable.");

        if (source.Statut != "Actif" || destination.Statut != "Actif")
            throw new InvalidOperationException("Les deux comptes doivent être actifs.");
        if (source.AgenceId == destination.AgenceId)
            throw new InvalidOperationException("Pour un virement intra-agence, utilisez le virement standard.");
        if (!string.Equals(source.Devise, destination.Devise, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Les devises des deux comptes doivent être identiques.");
        if (source.Solde < vm.Montant)
            throw new InvalidOperationException("Solde insuffisant.");

        var analyse = await _conformite.AnalyserTransactionAsync(source.Id, vm.Montant, "VirementInterAgence");
        if (analyse.EstBloquee)
            throw new InvalidOperationException(analyse.Motif ?? "Virement bloqué par le contrôle de conformité.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reference = GenererReference("VIR");
            var virement = new Virement
            {
                Reference = reference,
                CompteSourceId = source.Id,
                CompteDestinationId = destination.Id,
                UtilisateurId = utilisateurId,
                Montant = vm.Montant,
                Devise = source.Devise,
                Statut = "EnAttente",
                Motif = vm.Motif?.Trim(),
                ControleAML = analyse.NecessiteVigilanceRenforcee || analyse.NecessiteDeclaration,
                DateVirement = DateTime.UtcNow
            };
            _context.Virements.Add(virement);
            await _context.SaveChangesAsync();

            var virementInter = new VirementInterAgence
            {
                Reference = reference,
                VirementId = virement.Id,
                AgenceSourceId = source.AgenceId,
                AgenceDestinationId = destination.AgenceId,
                Montant = vm.Montant,
                Devise = source.Devise,
                Statut = "EnAttente",
                DateCreation = DateTime.UtcNow
            };
            _context.VirementsInterAgences.Add(virementInter);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return virementInter;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ValiderAsync(int virementInterAgenceId, int valideParId)
    {
        var transfert = await _context.VirementsInterAgences
            .Include(v => v.Virement).ThenInclude(v => v.CompteSource).ThenInclude(c => c.Agence)
            .Include(v => v.Virement).ThenInclude(v => v.CompteDestination).ThenInclude(c => c.Agence)
            .FirstOrDefaultAsync(v => v.Id == virementInterAgenceId);
        if (transfert is null || transfert.Statut != "EnAttente")
            return false;

        var virement = transfert.Virement;
        var source = virement.CompteSource;
        var destination = virement.CompteDestination;
        if (source.Statut != "Actif" || destination.Statut != "Actif")
            throw new InvalidOperationException("Un des comptes n'est plus actif.");
        if (source.Solde < transfert.Montant)
            throw new InvalidOperationException("Solde insuffisant sur le compte source.");
        if (source.AgenceId == destination.AgenceId)
            throw new InvalidOperationException("Les comptes ne sont pas situés dans deux agences différentes.");
        if (!string.Equals(source.Devise, destination.Devise, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Les devises des deux comptes doivent être identiques.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var maintenant = DateTime.UtcNow;
            var operation = new Operation
            {
                Reference = transfert.Reference,
                TypeOperation = "VirementInterAgence",
                CompteId = source.Id,
                UtilisateurId = valideParId,
                AgenceId = source.AgenceId,
                VirementId = virement.Id,
                Montant = transfert.Montant,
                Devise = transfert.Devise,
                Statut = "Valide",
                DateOperation = maintenant,
                DateValidation = maintenant,
                Commentaire = virement.Motif
            };
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            _context.Ecritures.AddRange(
                new Ecriture
                {
                    OperationId = operation.Id,
                    CompteId = source.Id,
                    CodeCompteGeneral = $"CLIENT:{source.NumeroCompte}",
                    Sens = "Debit",
                    Montant = transfert.Montant,
                    Devise = source.Devise,
                    DateEcriture = maintenant,
                    Libelle = $"Virement inter-agence vers {destination.NumeroCompte}"
                },
                new Ecriture
                {
                    OperationId = operation.Id,
                    CodeCompteGeneral = $"COMPENSATION:{source.Agence.Code}",
                    Sens = "Credit",
                    Montant = transfert.Montant,
                    Devise = source.Devise,
                    DateEcriture = maintenant,
                    Libelle = $"Compensation sortante {source.Agence.Code}"
                },
                new Ecriture
                {
                    OperationId = operation.Id,
                    CodeCompteGeneral = $"COMPENSATION:{destination.Agence.Code}",
                    Sens = "Debit",
                    Montant = transfert.Montant,
                    Devise = destination.Devise,
                    DateEcriture = maintenant,
                    Libelle = $"Compensation entrante {destination.Agence.Code}"
                },
                new Ecriture
                {
                    OperationId = operation.Id,
                    CompteId = destination.Id,
                    CodeCompteGeneral = $"CLIENT:{destination.NumeroCompte}",
                    Sens = "Credit",
                    Montant = transfert.Montant,
                    Devise = destination.Devise,
                    DateEcriture = maintenant,
                    Libelle = $"Virement inter-agence depuis {source.NumeroCompte}"
                });

            source.Solde -= transfert.Montant;
            destination.Solde += transfert.Montant;
            source.DateModification = destination.DateModification = maintenant;
            virement.Statut = "Valide";
            virement.DateValidation = maintenant;
            transfert.Statut = "EnCompensation";
            transfert.ValideParId = valideParId;
            transfert.DateCompensation = maintenant;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (virement.ControleAML)
                await CreerAlerteUniqueAsync(source.ClientId, operation.Id, "DepassementSeuil", "Moyen", "Virement inter-agence soumis à une vigilance renforcée.");
            await PublierAlertesAvanceesAsync(source.ClientId, operation.Id);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RejeterAsync(int virementInterAgenceId, int valideParId, string motif)
    {
        if (string.IsNullOrWhiteSpace(motif))
            throw new ArgumentException("Le motif du rejet est obligatoire.", nameof(motif));

        var transfert = await _context.VirementsInterAgences
            .Include(v => v.Virement)
            .FirstOrDefaultAsync(v => v.Id == virementInterAgenceId);
        if (transfert is null || transfert.Statut != "EnAttente")
            return false;

        transfert.Statut = "Rejete";
        transfert.MotifRejet = motif.Trim()[..Math.Min(motif.Trim().Length, 500)];
        transfert.ValideParId = valideParId;
        transfert.Virement.Statut = "Rejete";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<VirementInterAgence>> GetEnAttenteAsync(int? agenceId, int? utilisateurId = null)
    {
        var query = _context.VirementsInterAgences
            .Include(v => v.AgenceSource)
            .Include(v => v.AgenceDestination)
            .AsNoTracking()
            .Where(v => v.Statut == "EnAttente");
        if (agenceId.HasValue)
            query = query.Where(v => v.AgenceDestinationId == agenceId.Value
                || v.AgenceSourceId == agenceId.Value
                || (utilisateurId.HasValue && v.Virement.UtilisateurId == utilisateurId.Value));
        return await query.OrderByDescending(v => v.DateCreation).ToListAsync();
    }

    public async Task<IEnumerable<VirementInterAgence>> GetHistoriqueAsync(
        int? agenceId, int? utilisateurId, DateTime? du, DateTime? au)
    {
        var query = _context.VirementsInterAgences
            .Include(v => v.AgenceSource)
            .Include(v => v.AgenceDestination)
            .AsNoTracking()
            .AsQueryable();
        if (agenceId.HasValue)
            query = query.Where(v => v.AgenceSourceId == agenceId.Value
                || v.AgenceDestinationId == agenceId.Value
                || (utilisateurId.HasValue && v.Virement.UtilisateurId == utilisateurId.Value));
        if (du.HasValue)
        {
            var (debutUtc, _) = BankTime.GetUtcDayRange(du.Value);
            query = query.Where(v => v.DateCreation >= debutUtc);
        }
        if (au.HasValue)
        {
            var (_, finUtc) = BankTime.GetUtcDayRange(au.Value);
            query = query.Where(v => v.DateCreation < finUtc);
        }
        return await query.OrderByDescending(v => v.DateCreation).ToListAsync();
    }

    private async Task PublierAlertesAvanceesAsync(int clientId, int operationId)
    {
        var maintenant = DateTime.UtcNow;
        var resultats = new[]
        {
            await _detection.DetecterStructuringAsync(clientId, maintenant),
            await _detection.DetecterSmurfingAsync(clientId, maintenant),
            await _detection.DetecterLayeringAsync(clientId, maintenant),
            await _detection.DetecterVelociteAnormaleAsync(clientId, maintenant)
        };
        foreach (var alerte in resultats.SelectMany(r => r.Alertes))
            await CreerAlerteUniqueAsync(clientId, operationId, alerte.Regle, alerte.Niveau.ToString(), alerte.Description);
    }

    private async Task CreerAlerteUniqueAsync(int clientId, int operationId, string type, string niveau, string description)
    {
        var existe = await _context.AlertesAML.AnyAsync(a =>
            a.ClientId == clientId && a.TypeAlerte == type && a.Statut != "Traitee" && a.Statut != "Classee");
        if (!existe)
            await _declarations.CreerAlerteAsync(clientId, operationId, type, niveau, description);
    }

    private static string GenererReference(string prefix) =>
        $"{prefix}-{BankTime.Today:yyyyMMdd}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();
}
