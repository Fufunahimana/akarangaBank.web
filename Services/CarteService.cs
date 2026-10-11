using System.Security.Cryptography;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class CarteService : ICarteService
{
    private static readonly HashSet<string> TypesCarte = new(StringComparer.OrdinalIgnoreCase) { "Visa", "MasterCard", "Locale" };
    private static readonly HashSet<string> TypesTransaction = new(StringComparer.OrdinalIgnoreCase) { "Retrait", "Paiement", "PaiementEnLigne" };
    private static readonly HashSet<string> MotifsBlocage = new(StringComparer.OrdinalIgnoreCase) { "Perte", "Vol", "Fraude", "Autre" };

    private readonly AkarangaBankContext _context;
    private readonly IConformiteService? _conformite;
    private readonly IDeclarationService? _declarations;

    public CarteService(AkarangaBankContext context, IConformiteService? conformite = null, IDeclarationService? declarations = null)
    {
        _context = context;
        _conformite = conformite;
        _declarations = declarations;
    }

    public async Task<Carte> EmettreAsync(CarteEmissionViewModel demande)
    {
        var compte = await _context.Comptes.Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == demande.CompteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException("Une carte ne peut être émise que pour un compte actif.");
        if (!TypesCarte.Contains(demande.TypeCarte))
            throw new InvalidOperationException("Type de carte invalide.");
        if (demande.PlafondRetraitJournalier <= 0m || demande.PlafondPaiementJournalier <= 0m || demande.PlafondPaiementEnLigneJournalier <= 0m)
            throw new InvalidOperationException("Tous les plafonds doivent être supérieurs à zéro.");

        var type = CanonicaliserTypeCarte(demande.TypeCarte);
        var numero = await GenererNumeroUniqueAsync(type);
        var carte = new Carte
        {
            NumeroCarte = numero,
            NumeroCarteMasque = MasquerNumero(numero),
            CompteId = compte.Id,
            Compte = compte,
            ClientId = compte.ClientId,
            Client = compte.Client,
            TypeCarte = type,
            DateExpiration = DateTime.UtcNow.AddYears(3),
            PlafondRetraitJournalier = demande.PlafondRetraitJournalier,
            PlafondPaiementJournalier = demande.PlafondPaiementJournalier,
            PlafondPaiementEnLigneJournalier = demande.PlafondPaiementEnLigneJournalier,
            Statut = "Active",
            DateCreation = DateTime.UtcNow
        };
        _context.Cartes.Add(carte);
        await _context.SaveChangesAsync();
        return carte;
    }

    public async Task<Carte> BloquerAsync(int carteId, string motif)
    {
        if (string.IsNullOrWhiteSpace(motif) || !MotifsBlocage.Contains(motif.Trim()))
            throw new ArgumentException("Le motif doit être Perte, Vol, Fraude ou Autre.", nameof(motif));
        var carte = await _context.Cartes.FirstOrDefaultAsync(c => c.Id == carteId)
            ?? throw new InvalidOperationException("Carte introuvable.");
        if (carte.Statut != "Active")
            throw new InvalidOperationException("Seule une carte active peut être mise en opposition.");

        var raison = motif.Trim();
        carte.MotifBlocage = raison;
        carte.Statut = raison.ToUpperInvariant() switch
        {
            "PERTE" => "Perdue",
            "VOL" => "Volee",
            "FRAUDE" => "Opposition",
            _ => "Bloquee"
        };
        await _context.SaveChangesAsync();
        return carte;
    }

    public async Task<Carte> RenouvelerAsync(int carteId)
    {
        var ancienne = await _context.Cartes.FirstOrDefaultAsync(c => c.Id == carteId)
            ?? throw new InvalidOperationException("Carte introuvable.");
        if (ancienne.Statut == "Expiree")
            throw new InvalidOperationException("Cette carte a déjà été renouvelée ou expirée.");
        var compte = await _context.Comptes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == ancienne.CompteId)
            ?? throw new InvalidOperationException("Compte introuvable.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException("Le compte associé doit être actif pour renouveler la carte.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            ancienne.Statut = "Expiree";
            var numero = await GenererNumeroUniqueAsync(ancienne.TypeCarte);
            var nouvelle = new Carte
            {
                NumeroCarte = numero,
                NumeroCarteMasque = MasquerNumero(numero),
                CompteId = compte.Id,
                Compte = compte,
                ClientId = compte.ClientId,
                Client = compte.Client,
                TypeCarte = ancienne.TypeCarte,
                DateExpiration = DateTime.UtcNow.AddYears(3),
                PlafondRetraitJournalier = ancienne.PlafondRetraitJournalier,
                PlafondPaiementJournalier = ancienne.PlafondPaiementJournalier,
                PlafondPaiementEnLigneJournalier = ancienne.PlafondPaiementEnLigneJournalier,
                Statut = "Active",
                DateCreation = DateTime.UtcNow
            };
            _context.Cartes.Add(nouvelle);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return nouvelle;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TransactionCarte> AutoriserTransactionAsync(
        int carteId, decimal montant, string typeTransaction, int utilisateurId, DateTime? maintenant = null)
    {
        var carte = await _context.Cartes.Include(c => c.Compte).FirstOrDefaultAsync(c => c.Id == carteId)
            ?? throw new InvalidOperationException("Carte introuvable.");
        var now = maintenant ?? DateTime.UtcNow;
        var nowLocal = BankTime.ToLocal(now);
        var typeValide = CanonicaliserTypeTransaction(typeTransaction);
        var type = typeValide ?? (string.IsNullOrWhiteSpace(typeTransaction)
            ? "Invalide"
            : typeTransaction.Trim()[..Math.Min(30, typeTransaction.Trim().Length)]);

        string? motifRefus = typeValide is null ? "Type de transaction invalide." : null;
        if (motifRefus is null && montant <= 0m)
            motifRefus = "Le montant doit être supérieur à zéro.";
        else if (motifRefus is null && carte.Statut != "Active")
            motifRefus = $"Carte {carte.Statut}. Transaction refusée.";
        else if (motifRefus is null && carte.DateExpiration.Date < nowLocal.Date)
            motifRefus = "Carte expirée.";
        else if (motifRefus is null && carte.Compte.Statut != "Actif")
            motifRefus = "Le compte associé est inactif.";
        else if (motifRefus is null && carte.Compte.Solde < montant)
            motifRefus = "Solde insuffisant.";

        if (motifRefus is null)
        {
            var (debutJournee, finJournee) = BankTime.GetUtcDayRange(nowLocal);
            if (type == "Retrait")
            {
                var utilise = await GetUtilisationJournaliereAsync(carte.Id, debutJournee, finJournee, "Retrait");
                if (utilise + montant > carte.PlafondRetraitJournalier)
                    motifRefus = $"Plafond journalier de retrait dépassé ({carte.PlafondRetraitJournalier:N2} BIF).";
            }
            else
            {
                var utiliseEnLigne = type == "PaiementEnLigne"
                    ? await GetUtilisationJournaliereAsync(carte.Id, debutJournee, finJournee, "PaiementEnLigne")
                    : 0m;
                var plafondEnLigneDepasse = type == "PaiementEnLigne"
                    && utiliseEnLigne + montant > carte.PlafondPaiementEnLigneJournalier;
                var utilisePaiements = await GetUtilisationJournaliereAsync(carte.Id, debutJournee, finJournee, "Paiement", "PaiementEnLigne");
                var plafondPaiementDepasse = utilisePaiements + montant > carte.PlafondPaiementJournalier;
                if (plafondEnLigneDepasse)
                    motifRefus = $"Plafond journalier de paiement en ligne dépassé ({carte.PlafondPaiementEnLigneJournalier:N2} BIF).";
                else if (plafondPaiementDepasse)
                    motifRefus = $"Plafond journalier de paiement dépassé ({carte.PlafondPaiementJournalier:N2} BIF).";
            }
        }

        ResultatConformite? analyseAML = null;
        if (motifRefus is null && _conformite is not null)
        {
            var typeAML = type == "Retrait" ? "Retrait" : "PaiementCarte";
            analyseAML = await _conformite.AnalyserTransactionAsync(carte.CompteId, montant, typeAML);
            if (analyseAML.EstBloquee)
                motifRefus = analyseAML.Motif ?? "Transaction bloquée par le contrôle AML.";
        }

        if (motifRefus is not null)
        {
            var refusee = new TransactionCarte
            {
                CarteId = carte.Id,
                TypeTransaction = type,
                Montant = montant,
                Statut = "Refusee",
                MotifRefus = motifRefus,
                DateTransaction = now
            };
            _context.TransactionsCarte.Add(refusee);
            await _context.SaveChangesAsync();
            await CreerAlerteAMLAsync(analyseAML, carte.ClientId, null);
            return refusee;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var operation = new Operation
            {
                Reference = $"CB-{nowLocal:yyyyMMdd}-{RandomNumberGenerator.GetHexString(6)}",
                TypeOperation = $"Carte{type}",
                CompteId = carte.CompteId,
                UtilisateurId = utilisateurId,
                AgenceId = carte.Compte.AgenceId,
                Montant = montant,
                Devise = carte.Compte.Devise,
                Statut = "Valide",
                DateOperation = now,
                DateValidation = now,
                Commentaire = $"Transaction carte {carte.NumeroCarteMasque} ({type})"
            };
            var transactionCarte = new TransactionCarte
            {
                CarteId = carte.Id,
                Carte = carte,
                TypeTransaction = type,
                Montant = montant,
                Statut = "Autorisee",
                DateTransaction = now,
                Operation = operation
            };
            operation.Ecritures.Add(new Ecriture
            {
                CompteId = carte.CompteId,
                CodeCompteGeneral = $"CLIENT:{carte.Compte.NumeroCompte}",
                Sens = "Debit",
                Montant = montant,
                Devise = carte.Compte.Devise,
                DateEcriture = now,
                Libelle = $"Transaction carte {carte.NumeroCarteMasque}"
            });
            operation.Ecritures.Add(new Ecriture
            {
                CodeCompteGeneral = $"COMPENSATION_CARTES:{carte.Compte.AgenceId}",
                Sens = "Credit",
                Montant = montant,
                Devise = carte.Compte.Devise,
                DateEcriture = now,
                Libelle = $"Contrepartie transaction carte {carte.NumeroCarteMasque}"
            });

            carte.Compte.Solde -= montant;
            carte.Compte.DateModification = now;
            _context.Operations.Add(operation);
            _context.TransactionsCarte.Add(transactionCarte);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await CreerAlerteAMLAsync(analyseAML, carte.ClientId, operation.Id);
            return transactionCarte;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<TransactionCarte>> GetHistoriqueAsync(int carteId) =>
        await _context.TransactionsCarte.AsNoTracking().Include(t => t.Operation).Where(t => t.CarteId == carteId)
            .OrderByDescending(t => t.DateTransaction).ToListAsync();

    private async Task<decimal> GetUtilisationJournaliereAsync(int carteId, DateTime debut, DateTime fin, params string[] types) =>
        await _context.TransactionsCarte
            .Where(t => t.CarteId == carteId && t.Statut == "Autorisee" && types.Contains(t.TypeTransaction)
                && t.DateTransaction >= debut && t.DateTransaction < fin)
            .SumAsync(t => (decimal?)t.Montant)
            ?? 0m;

    private async Task CreerAlerteAMLAsync(ResultatConformite? analyse, int clientId, int? operationId)
    {
        if (_declarations is null || analyse is null
            || (!analyse.NecessiteVigilanceRenforcee && !analyse.NecessiteDeclaration && !analyse.EstBloquee))
            return;

        await _declarations.CreerAlerteAsync(
            clientId,
            operationId,
            "DepassementSeuil",
            analyse.Niveau.ToString(),
            analyse.Motif ?? "Transaction carte nécessitant une vigilance AML.");
    }

    public static string MasquerNumero(string numero) => $"**** **** **** {numero[^4..]}";

    private async Task<string> GenererNumeroUniqueAsync(string typeCarte)
    {
        var prefixe = typeCarte switch
        {
            "Visa" => "4",
            "MasterCard" => "5",
            _ => "6"
        };

        for (var tentative = 0; tentative < 10; tentative++)
        {
            var numero = prefixe + string.Concat(Enumerable.Range(0, 15)
                .Select(_ => RandomNumberGenerator.GetInt32(0, 10).ToString()));
            if (!await _context.Cartes.AnyAsync(c => c.NumeroCarte == numero))
                return numero;
        }
        throw new InvalidOperationException("Impossible de générer un numéro de carte unique.");
    }

    private static string CanonicaliserTypeCarte(string type) => type.Trim().ToUpperInvariant() switch
    {
        "VISA" => "Visa",
        "MASTERCARD" => "MasterCard",
        "LOCALE" => "Locale",
        _ => throw new InvalidOperationException("Type de carte invalide.")
    };

    private static string? CanonicaliserTypeTransaction(string? type) => type?.Trim().ToUpperInvariant() switch
    {
        "RETRAIT" => "Retrait",
        "PAIEMENT" => "Paiement",
        "PAIEMENTENLIGNE" or "PAIEMENT EN LIGNE" => "PaiementEnLigne",
        _ => null
    };
}
