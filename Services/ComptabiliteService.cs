using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AkarangaBank.Web.Services;

public class ComptabiliteService : IComptabiliteService
{
    private const decimal TauxFraisRetrait = 0.01m;
    private const decimal FraisRetraitMinimum = 100m;

    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;
    private readonly ISurveillanceService _surveillance;
    private readonly IDeclarationService _declaration;
    private readonly IDetectionAvanceeService? _detection;

    public ComptabiliteService(
        AkarangaBankContext context,
        IConformiteService conformite,
        ISurveillanceService surveillance,
        IDeclarationService declaration,
        IDetectionAvanceeService? detection = null)
    {
        _context = context;
        _conformite = conformite;
        _surveillance = surveillance;
        _declaration = declaration;
        _detection = detection;
    }

    public async Task<Operation> DeposerAsync(DepotViewModel vm, int utilisateurId)
    {
        var compte = await GetCompteActifAsync(vm.CompteId);
        var (analyse, fractionnement, surveillance) = await AnalyserAsync(vm.CompteId, vm.Montant, "Depot");
        if (analyse.EstBloquee)
            throw new InvalidOperationException(analyse.Motif ?? "Opération bloquée par le contrôle de conformité.");

        var approbationAdminRequise = RequiertApprobationAdmin(analyse);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var transactionCommitted = false;
        try
        {
            var operation = CreerOperation(compte, utilisateurId, "Depot", vm.Montant, vm.Commentaire);
            if (approbationAdminRequise)
            {
                operation.Statut = "EnAttente";
                operation.DateValidation = null;
            }
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            if (!approbationAdminRequise)
            {
                AjouterEcrituresDepot(operation, compte);
                compte.Solde += vm.Montant;
                compte.DateModification = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            transactionCommitted = true;

            await CreerAlertesSiNecessaireAsync(compte.ClientId, operation.Id, analyse, fractionnement, surveillance);
            await CreerAlertesAvanceesAsync(compte.ClientId, operation.Id);
            return operation;
        }
        catch
        {
            if (!transactionCommitted)
                await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Operation> RetirerAsync(RetraitViewModel vm, int utilisateurId)
    {
        var compte = await GetCompteActifAsync(vm.CompteId);
        var frais = CalculerFraisRetrait(vm.Montant);
        if (compte.Solde < vm.Montant + frais)
            throw new InvalidOperationException("Solde insuffisant.");

        var (analyse, fractionnement, surveillance) = await AnalyserAsync(vm.CompteId, vm.Montant, "Retrait");
        if (analyse.EstBloquee)
            throw new InvalidOperationException(analyse.Motif ?? "Opération bloquée par le contrôle de conformité.");

        var approbationAdminRequise = RequiertApprobationAdmin(analyse);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var transactionCommitted = false;
        try
        {
            var operation = CreerOperation(compte, utilisateurId, "Retrait", vm.Montant, vm.Commentaire);
            Operation? operationFrais = null;
            if (approbationAdminRequise)
            {
                operation.Statut = "EnAttente";
                operation.DateValidation = null;
            }
            _context.Operations.Add(operation);
            if (!approbationAdminRequise)
            {
                operationFrais = CreerOperation(compte, utilisateurId, "Frais", frais, "Frais de retrait (1 %, minimum 100 BIF)");
                _context.Operations.Add(operationFrais);
            }
            await _context.SaveChangesAsync();

            if (!approbationAdminRequise)
            {
                AjouterEcrituresRetrait(operation, operationFrais!, compte);
                compte.Solde -= vm.Montant + frais;
                compte.DateModification = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            transactionCommitted = true;

            await CreerAlertesSiNecessaireAsync(compte.ClientId, operation.Id, analyse, fractionnement, surveillance);
            await CreerAlertesAvanceesAsync(compte.ClientId, operation.Id);
            return operation;
        }
        catch
        {
            if (!transactionCommitted)
                await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Operation?> PrendreOperationAmlEnAttenteAsync(int operationId)
    {
        var enAttente = _context.Operations
            .Where(o => o.Id == operationId && o.Statut == "EnAttente"
                && (o.TypeOperation == "Depot" || o.TypeOperation == "Retrait"));

        if (_context.Database.IsRelational())
        {
            var reservees = await enAttente.ExecuteUpdateAsync(
                setters => setters.SetProperty(o => o.Statut, "Traitement"));
            if (reservees == 0)
                return null;
        }

        var operation = await _context.Operations
            .Include(o => o.Compte)
            .FirstOrDefaultAsync(o => o.Id == operationId
                && (o.TypeOperation == "Depot" || o.TypeOperation == "Retrait"));
        if (operation is null || (operation.Statut != "EnAttente" && operation.Statut != "Traitement"))
            return null;

        operation.Statut = "Traitement";
        return operation;
    }

    private void AjouterEcrituresDepot(Operation operation, Compte compte)
    {
        var maintenant = DateTime.UtcNow;
        _context.Ecritures.AddRange(new Ecriture
        {
            OperationId = operation.Id,
            CompteId = compte.Id,
            CodeCompteGeneral = $"CLIENT:{compte.NumeroCompte}",
            Sens = "Credit",
            Montant = operation.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Dépôt sur compte {compte.NumeroCompte}"
        }, new Ecriture
        {
            OperationId = operation.Id,
            CompteId = null,
            CodeCompteGeneral = $"CAISSE:{compte.AgenceId}",
            Sens = "Debit",
            Montant = operation.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Contrepartie de caisse du dépôt {operation.Reference}"
        });
    }

    private void AjouterEcrituresRetrait(Operation operation, Operation operationFrais, Compte compte)
    {
        var maintenant = DateTime.UtcNow;
        _context.Ecritures.AddRange(new Ecriture
        {
            OperationId = operation.Id,
            CompteId = compte.Id,
            CodeCompteGeneral = $"CLIENT:{compte.NumeroCompte}",
            Sens = "Debit",
            Montant = operation.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Retrait du compte {compte.NumeroCompte}"
        }, new Ecriture
        {
            OperationId = operationFrais.Id,
            CompteId = compte.Id,
            CodeCompteGeneral = $"CLIENT:{compte.NumeroCompte}",
            Sens = "Debit",
            Montant = operationFrais.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Frais de retrait du compte {compte.NumeroCompte}"
        }, new Ecriture
        {
            OperationId = operation.Id,
            CompteId = null,
            CodeCompteGeneral = $"CAISSE:{compte.AgenceId}",
            Sens = "Credit",
            Montant = operation.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Contrepartie de caisse du retrait {operation.Reference}"
        }, new Ecriture
        {
            OperationId = operationFrais.Id,
            CompteId = null,
            CodeCompteGeneral = $"PRODUITS_FRAIS:{compte.AgenceId}",
            Sens = "Credit",
            Montant = operationFrais.Montant,
            Devise = compte.Devise,
            DateEcriture = maintenant,
            Libelle = $"Contrepartie de produit des frais {operationFrais.Reference}"
        });
    }

    public async Task<IEnumerable<Operation>> GetOperationsAmlEnAttenteAsync() =>
        await _context.Operations.AsNoTracking()
            .Include(o => o.Compte).ThenInclude(c => c.Client)
            .Include(o => o.Utilisateur)
            .Where(o => o.Statut == "EnAttente" && (o.TypeOperation == "Depot" || o.TypeOperation == "Retrait"))
            .OrderBy(o => o.DateOperation)
            .ToListAsync();

    public async Task<bool> ValiderOperationAmlAsync(int operationId, int valideParId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var operation = await PrendreOperationAmlEnAttenteAsync(operationId);
        if (operation is null)
            return false;

        var clientId = operation.Compte.ClientId;
        try
        {
            var compte = operation.Compte;
            if (compte.Statut != "Actif")
                throw new InvalidOperationException("Le compte n'est plus actif.");

            if (operation.TypeOperation == "Depot")
            {
                AjouterEcrituresDepot(operation, compte);
                compte.Solde += operation.Montant;
            }
            else
            {
                var frais = CalculerFraisRetrait(operation.Montant);
                if (compte.Solde < operation.Montant + frais)
                    throw new InvalidOperationException("Solde insuffisant au moment de l'approbation du retrait.");

                var operationFrais = CreerOperation(
                    compte, operation.UtilisateurId, "Frais", frais, "Frais de retrait (1 %, minimum 100 BIF)");
                _context.Operations.Add(operationFrais);
                await _context.SaveChangesAsync();
                AjouterEcrituresRetrait(operation, operationFrais, compte);
                compte.Solde -= operation.Montant + frais;
            }

            var maintenant = DateTime.UtcNow;
            compte.DateModification = maintenant;
            operation.Statut = "Valide";
            operation.DateValidation = maintenant;
            _context.AuditLogs.Add(CreerAuditDecisionAml(operation, valideParId, "ValidationOperationAML", "Valide"));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await CreerAlertesAvanceesAsync(clientId, operation.Id);
        return true;
    }

    public async Task<bool> RejeterOperationAmlAsync(int operationId, int valideParId, string motif)
    {
        if (string.IsNullOrWhiteSpace(motif))
            throw new ArgumentException("Le motif du rejet est obligatoire.", nameof(motif));

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var operation = await PrendreOperationAmlEnAttenteAsync(operationId);
        if (operation is null)
            return false;

        try
        {
            var maintenant = DateTime.UtcNow;
            operation.Statut = "Rejete";
            operation.DateValidation = maintenant;
            operation.Commentaire = LimiterCommentaire(
                $"{operation.Commentaire} | Rejet AML : {motif.Trim()}");
            _context.AuditLogs.Add(CreerAuditDecisionAml(operation, valideParId, "RejetOperationAML", "Rejete"));
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

    public async Task<Operation> VirerAsync(VirementViewModel vm, int utilisateurId)
    {
        if (vm.CompteSourceId == vm.CompteDestinationId)
            throw new InvalidOperationException("Les comptes source et destination doivent être différents.");

        var source = await GetCompteActifAsync(vm.CompteSourceId);
        var destination = await GetCompteActifAsync(vm.CompteDestinationId);
        if (source.AgenceId != destination.AgenceId)
            throw new InvalidOperationException("Un virement entre deux agences doit rester en attente et être approuvé par un administrateur.");
        if (source.Devise != destination.Devise)
            throw new InvalidOperationException("Les devises des deux comptes doivent être identiques.");
        if (source.Solde < vm.Montant)
            throw new InvalidOperationException("Solde insuffisant sur le compte source.");

        var (analyse, fractionnement, surveillance) = await AnalyserAsync(vm.CompteSourceId, vm.Montant, "Virement");
        if (analyse.EstBloquee)
            throw new InvalidOperationException(analyse.Motif ?? "Opération bloquée par le contrôle de conformité.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var transactionCommitted = false;
        try
        {
            var virement = new Virement
            {
                Reference = GenererReference("VIR"),
                CompteSourceId = source.Id,
                CompteDestinationId = destination.Id,
                UtilisateurId = utilisateurId,
                Montant = vm.Montant,
                Devise = source.Devise,
                Statut = "Valide",
                Motif = vm.Motif?.Trim(),
                ControleAML = analyse.NecessiteVigilanceRenforcee || fractionnement,
                DateVirement = DateTime.UtcNow,
                DateValidation = DateTime.UtcNow
            };
            _context.Virements.Add(virement);
            await _context.SaveChangesAsync();

            var operation = CreerOperation(source, utilisateurId, "Virement", vm.Montant, vm.Motif);
            operation.Reference = virement.Reference;
            operation.VirementId = virement.Id;
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            var ecritures = new[]
            {
                new Ecriture
                {
                    OperationId = operation.Id, CompteId = source.Id, CodeCompteGeneral = $"CLIENT:{source.NumeroCompte}", Sens = "Debit",
                    Montant = vm.Montant, Devise = source.Devise, DateEcriture = DateTime.UtcNow,
                    Libelle = $"Virement sortant vers {destination.NumeroCompte}"
                },
                new Ecriture
                {
                    OperationId = operation.Id, CompteId = destination.Id, CodeCompteGeneral = $"CLIENT:{destination.NumeroCompte}", Sens = "Credit",
                    Montant = vm.Montant, Devise = destination.Devise, DateEcriture = DateTime.UtcNow,
                    Libelle = $"Virement entrant depuis {source.NumeroCompte}"
                }
            };

            VerifierEquilibre(ecritures);
            _context.Ecritures.AddRange(ecritures);
            source.Solde -= vm.Montant;
            destination.Solde += vm.Montant;
            source.DateModification = destination.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            transactionCommitted = true;
            await CreerAlertesSiNecessaireAsync(source.ClientId, operation.Id, analyse, fractionnement, surveillance);
            await CreerAlertesAvanceesAsync(source.ClientId, operation.Id);
            return operation;
        }
        catch
        {
            if (!transactionCommitted)
                await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<JournalViewModel> GetJournalAsync(
        DateTime? du, DateTime? au, string? typeOperation, int page, int taille)
    {
        page = Math.Max(page, 1);
        taille = Math.Clamp(taille, 1, 100);
        var query = _context.Operations.AsNoTracking()
            .Include(o => o.Compte)
            .Include(o => o.Ecritures)
            .Include(o => o.Utilisateur)
            .AsQueryable();

        if (du.HasValue)
        {
            var (debutUtc, _) = BankTime.GetUtcDayRange(du.Value);
            query = query.Where(o => o.DateOperation >= debutUtc);
        }
        if (au.HasValue)
        {
            var (_, finUtc) = BankTime.GetUtcDayRange(au.Value);
            query = query.Where(o => o.DateOperation < finUtc);
        }
        if (!string.IsNullOrWhiteSpace(typeOperation))
            query = query.Where(o => o.TypeOperation == typeOperation);

        var total = await query.CountAsync();
        var operations = await query.OrderByDescending(o => o.DateOperation)
            .Skip((page - 1) * taille)
            .Take(taille)
            .ToListAsync();

        return new JournalViewModel
        {
            Operations = operations,
            Du = du,
            Au = au,
            TypeOperation = typeOperation,
            Page = page,
            TaillePage = taille,
            Total = total
        };
    }

    public async Task<ReleveViewModel?> GetReleveAsync(int compteId)
    {
        var compte = await _context.Comptes.AsNoTracking()
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == compteId);
        if (compte is null)
            return null;

        var ecritures = await _context.Ecritures.AsNoTracking()
            .Where(e => e.CompteId == compteId)
            .OrderBy(e => e.DateEcriture)
            .ToListAsync();
        var credits = ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
        var debits = ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant);

        return new ReleveViewModel
        {
            Compte = compte,
            SoldeInitial = compte.Solde - credits + debits,
            SoldeFinal = compte.Solde,
            Ecritures = ecritures
        };
    }

    public async Task<decimal> GetSoldeAsync(int compteId)
    {
        return await _context.Comptes.AsNoTracking()
            .Where(c => c.Id == compteId)
            .Select(c => (decimal?)c.Solde)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Compte introuvable.");
    }

    public async Task<bool> VerifierEquilibreGlobalAsync()
    {
        var operations = await _context.Operations.AsNoTracking()
            .Where(o => o.Statut == "Valide")
            .Select(o => new
            {
                o.TypeOperation,
                o.Montant,
                NombreEcritures = o.Ecritures.Count,
                Debits = o.Ecritures.Where(e => e.Sens == "Debit").Sum(e => (decimal?)e.Montant) ?? 0m,
                Credits = o.Ecritures.Where(e => e.Sens == "Credit").Sum(e => (decimal?)e.Montant) ?? 0m
            })
            .ToListAsync();

        return operations.All(operation => operation.TypeOperation switch
        {
            "Depot" => operation.NombreEcritures == 2 && operation.Debits == operation.Montant && operation.Credits == operation.Montant,
            "Retrait" or "Frais" => operation.NombreEcritures == 2 && operation.Debits == operation.Montant && operation.Credits == operation.Montant,
            "Virement" => operation.NombreEcritures == 2 && operation.Debits == operation.Montant && operation.Credits == operation.Montant,
            _ => operation.NombreEcritures > 0 && operation.Debits == operation.Credits
        });
    }

    private async Task<Compte> GetCompteActifAsync(int compteId)
    {
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == compteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException($"Compte {compte.Statut}. Opération impossible.");
        return compte;
    }

    private async Task<(ResultatConformite Analyse, bool Fractionnement, ResultatSurveillance Surveillance)> AnalyserAsync(
        int compteId, decimal montant, string typeOperation)
    {
        var date = DateTime.UtcNow;
        var analyse = await _conformite.AnalyserTransactionAsync(compteId, montant, typeOperation);
        var fractionnement = await _conformite.DetecterFractionnementAsync(compteId, montant, date);
        var surveillance = await _surveillance.AnalyserTransactionAsync(compteId, montant, typeOperation, date);
        return (analyse, fractionnement, surveillance);
    }

    private static Operation CreerOperation(Compte compte, int utilisateurId, string type, decimal montant, string? commentaire)
    {
        var now = DateTime.UtcNow;
        return new Operation
        {
            Reference = GenererReference("AKR"),
            TypeOperation = type,
            CompteId = compte.Id,
            UtilisateurId = utilisateurId,
            AgenceId = compte.AgenceId,
            Montant = montant,
            Devise = compte.Devise,
            Statut = "Valide",
            DateOperation = now,
            DateValidation = now,
            Commentaire = commentaire?.Trim()
        };
    }

    private static bool RequiertApprobationAdmin(ResultatConformite analyse) =>
        analyse.NecessiteVigilanceRenforcee || analyse.NecessiteDeclaration;

    private static AuditLog CreerAuditDecisionAml(Operation operation, int adminId, string action, string statut) =>
        new()
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = adminId,
            AgenceId = operation.AgenceId,
            Action = action,
            TableConcernee = "Operations",
            EnregistrementId = operation.Id,
            ValeurAvant = "EnAttente",
            ValeurApres = statut,
            Details = $"Décision admin sur l'opération AML {operation.Reference}."
        };

    private static string? LimiterCommentaire(string commentaire) =>
        commentaire.Length <= 255 ? commentaire : commentaire[..255];

    private async Task CreerAlertesSiNecessaireAsync(
        int clientId, int operationId, ResultatConformite analyse, bool fractionnement, ResultatSurveillance surveillance)
    {
        if (analyse.NecessiteVigilanceRenforcee || analyse.NecessiteDeclaration)
        {
            await _declaration.CreerAlerteAsync(
                clientId, operationId, "DepassementSeuil", analyse.Niveau.ToString(),
                analyse.Motif ?? "Dépassement du seuil AML.");
        }

        if (fractionnement)
        {
            await _declaration.CreerAlerteAsync(
                clientId, operationId, "Fractionnement", "Eleve",
                "Suspicion de fractionnement : plusieurs opérations sous le seuil sur une courte période.");
        }

        foreach (var alerte in surveillance.Alertes.Where(a =>
            a.Regle != "MontantEleve"
            && !(analyse.NecessiteVigilanceRenforcee && a.Regle == "MontantInhabituel")))
        {
            await _declaration.CreerAlerteAsync(
                clientId, operationId, alerte.Regle, alerte.Niveau.ToString(), alerte.Description);
        }
    }

    private async Task CreerAlertesAvanceesAsync(int clientId, int operationId)
    {
        if (_detection is null)
            return;

        var maintenant = DateTime.UtcNow;
        var resultats = new[]
        {
            await _detection.DetecterStructuringAsync(clientId, maintenant),
            await _detection.DetecterSmurfingAsync(clientId, maintenant),
            await _detection.DetecterLayeringAsync(clientId, maintenant),
            await _detection.DetecterVelociteAnormaleAsync(clientId, maintenant)
        };

        foreach (var alerte in resultats.SelectMany(r => r.Alertes))
        {
            var existe = await _context.AlertesAML.AnyAsync(a =>
                a.ClientId == clientId && a.TypeAlerte == alerte.Regle
                && a.Statut != "Traitee" && a.Statut != "Classee");
            if (!existe)
                await _declaration.CreerAlerteAsync(clientId, operationId, alerte.Regle, alerte.Niveau.ToString(), alerte.Description);
        }
    }

    private static string GenererReference(string prefix)
    {
        var date = BankTime.Today.ToString("yyyyMMdd");
        var random = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{prefix}-{date}-{random}";
    }

    private static decimal CalculerFraisRetrait(decimal montant)
    {
        var fraisPourcentage = decimal.Round(montant * TauxFraisRetrait, 2, MidpointRounding.AwayFromZero);
        return Math.Max(FraisRetraitMinimum, fraisPourcentage);
    }

    private static void VerifierEquilibre(IEnumerable<Ecriture> ecritures)
    {
        var debit = ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
        var credit = ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
        if (debit != credit)
            throw new InvalidOperationException($"Écriture déséquilibrée : débit={debit:N2}, crédit={credit:N2}.");
    }
}
