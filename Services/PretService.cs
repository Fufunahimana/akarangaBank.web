using System.Security.Cryptography;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class PretService : IPretService
{
    private const decimal TauxPenaliteMensuel = 0.005m;
    private const decimal PenaliteMinimum = 500m;
    private readonly AkarangaBankContext _context;

    public PretService(AkarangaBankContext context) => _context = context;

    public decimal CalculerMensualite(decimal principal, decimal tauxAnnuel, int dureeMois)
    {
        if (principal <= 0m) throw new ArgumentOutOfRangeException(nameof(principal), "Le montant du prêt doit être positif.");
        if (tauxAnnuel < 0m) throw new ArgumentOutOfRangeException(nameof(tauxAnnuel), "Le taux ne peut pas être négatif.");
        if (dureeMois <= 0) throw new ArgumentOutOfRangeException(nameof(dureeMois), "La durée doit être positive.");

        var tauxMensuel = tauxAnnuel / 1200m;
        if (tauxMensuel == 0m)
            return decimal.Round(principal / dureeMois, 2, MidpointRounding.AwayFromZero);

        var facteur = 1m;
        for (var i = 0; i < dureeMois; i++)
            facteur *= 1m + tauxMensuel;

        return decimal.Round(principal * tauxMensuel * facteur / (facteur - 1m), 2, MidpointRounding.AwayFromZero);
    }

    public IReadOnlyList<EcheancePret> GenererEcheancier(Pret pret, DateTime? dateDebut = null)
    {
        ArgumentNullException.ThrowIfNull(pret);
        if (pret.MontantPrincipal <= 0m || pret.DureeMois <= 0)
            throw new ArgumentException("Le prêt doit avoir un montant et une durée positifs.", nameof(pret));

        var mensualite = pret.Mensualite > 0m
            ? pret.Mensualite
            : CalculerMensualite(pret.MontantPrincipal, pret.TauxInteret, pret.DureeMois);
        var tauxMensuel = pret.TauxInteret / 1200m;
        var solde = pret.MontantPrincipal;
        var dateReference = (dateDebut ?? pret.DateDebut ?? BankTime.Today).Date;
        var echeances = new List<EcheancePret>(pret.DureeMois);

        for (var numero = 1; numero <= pret.DureeMois; numero++)
        {
            var interet = decimal.Round(solde * tauxMensuel, 2, MidpointRounding.AwayFromZero);
            var capital = numero == pret.DureeMois
                ? solde
                : Math.Min(solde, decimal.Round(mensualite - interet, 2, MidpointRounding.AwayFromZero));
            solde = Math.Max(0m, solde - capital);
            echeances.Add(new EcheancePret
            {
                PretId = pret.Id,
                Pret = pret,
                NumeroEcheance = numero,
                Capital = capital,
                Interet = interet,
                MontantTotal = capital + interet,
                SoldeRestant = solde,
                DateEcheance = dateReference.AddMonths(numero),
                Statut = "EnAttente"
            });
        }

        return echeances;
    }

    public async Task<Pret> CreerDemandeAsync(PretDemandeViewModel demande)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == demande.ClientId && c.Statut == "Actif")
            ?? throw new InvalidOperationException("Client introuvable ou inactif.");
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == demande.CompteDebitId && c.ClientId == demande.ClientId && c.Statut == "Actif")
            ?? throw new InvalidOperationException("Le compte doit appartenir au client et être actif.");
        var type = await _context.TypesPret.FirstOrDefaultAsync(t => t.Id == demande.TypePretId && t.Actif)
            ?? throw new InvalidOperationException("Type de prêt introuvable ou inactif.");

        if (demande.MontantPrincipal < type.MontantMin || demande.MontantPrincipal > type.MontantMax)
            throw new InvalidOperationException($"Le montant doit être compris entre {type.MontantMin:N2} et {type.MontantMax:N2} BIF.");
        if (demande.DureeMois < type.DureeMinMois || demande.DureeMois > type.DureeMaxMois)
            throw new InvalidOperationException($"La durée doit être comprise entre {type.DureeMinMois} et {type.DureeMaxMois} mois.");

        var taux = (type.TauxMin + type.TauxMax) / 2m;
        var pret = new Pret
        {
            Reference = GenererReference("PRET"),
            ClientId = client.Id,
            Client = client,
            TypePretId = type.Id,
            TypePret = type,
            CompteDebitId = compte.Id,
            CompteDebit = compte,
            MontantPrincipal = demande.MontantPrincipal,
            TauxInteret = taux,
            DureeMois = demande.DureeMois,
            Mensualite = CalculerMensualite(demande.MontantPrincipal, taux, demande.DureeMois),
            SoldeRestant = demande.MontantPrincipal,
            Statut = "EnAttente",
            DateDemande = DateTime.UtcNow,
            Commentaire = string.IsNullOrWhiteSpace(demande.Commentaire) ? null : demande.Commentaire.Trim()
        };
        _context.Prets.Add(pret);
        await _context.SaveChangesAsync();
        return pret;
    }

    public async Task<Pret> ValiderAsync(int pretId, decimal tauxPersonnalise, int utilisateurId)
    {
        var pret = await _context.Prets.Include(p => p.TypePret).FirstOrDefaultAsync(p => p.Id == pretId)
            ?? throw new InvalidOperationException("Prêt introuvable.");
        if (pret.Statut != "EnAttente")
            throw new InvalidOperationException("Seule une demande en attente peut être validée.");
        if (tauxPersonnalise < pret.TypePret.TauxMin || tauxPersonnalise > pret.TypePret.TauxMax)
            throw new InvalidOperationException($"Le taux doit être compris entre {pret.TypePret.TauxMin:N2} % et {pret.TypePret.TauxMax:N2} %.");

        pret.TauxInteret = tauxPersonnalise;
        pret.Mensualite = CalculerMensualite(pret.MontantPrincipal, tauxPersonnalise, pret.DureeMois);
        pret.Statut = "Valide";
        pret.ValideParId = utilisateurId;
        pret.DateValidation = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return pret;
    }

    public async Task<Pret> RejeterAsync(int pretId, string motif)
    {
        if (string.IsNullOrWhiteSpace(motif))
            throw new ArgumentException("Le motif du rejet est obligatoire.", nameof(motif));
        var pret = await _context.Prets.FirstOrDefaultAsync(p => p.Id == pretId)
            ?? throw new InvalidOperationException("Prêt introuvable.");
        if (pret.Statut != "EnAttente")
            throw new InvalidOperationException("Seule une demande en attente peut être rejetée.");

        pret.Statut = "Rejete";
        pret.MotifRejet = motif.Trim();
        await _context.SaveChangesAsync();
        return pret;
    }

    public async Task<Operation> DebloquerAsync(int pretId, int utilisateurId)
    {
        var pret = await _context.Prets.Include(p => p.CompteDebit)
            .FirstOrDefaultAsync(p => p.Id == pretId)
            ?? throw new InvalidOperationException("Prêt introuvable.");
        if (pret.Statut != "Valide")
            throw new InvalidOperationException("Le prêt doit être validé avant son déblocage.");
        if (pret.CompteDebit.Statut != "Actif")
            throw new InvalidOperationException("Le compte de débit est inactif.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var now = DateTime.UtcNow;
            var operation = new Operation
            {
                Reference = GenererReference("DEB"),
                TypeOperation = "DeblocagePret",
                CompteId = pret.CompteDebitId,
                UtilisateurId = utilisateurId,
                AgenceId = pret.CompteDebit.AgenceId,
                PretId = pret.Id,
                Montant = pret.MontantPrincipal,
                Devise = pret.CompteDebit.Devise,
                Statut = "Valide",
                DateOperation = now,
                DateValidation = now,
                Commentaire = $"Déblocage du prêt {pret.Reference}"
            };
            pret.CompteDebit.Solde += pret.MontantPrincipal;
            pret.CompteDebit.DateModification = now;
            pret.Statut = "EnCours";
            pret.DateDebut = now;
            pret.DateFin = now.AddMonths(pret.DureeMois);
            pret.SoldeRestant = pret.MontantPrincipal;
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            var echeances = GenererEcheancier(pret, now);
            _context.EcheancesPret.AddRange(echeances);
            _context.Ecritures.AddRange(
                CreerEcriture(operation.Id, pret.CompteDebitId, $"CLIENT:{pret.CompteDebit.NumeroCompte}", "Credit", pret.MontantPrincipal, pret.CompteDebit.Devise, $"Déblocage du prêt {pret.Reference}"),
                CreerEcriture(operation.Id, null, $"PRETS_ENCOURS:{pret.Reference}", "Debit", pret.MontantPrincipal, pret.CompteDebit.Devise, $"Créance du prêt {pret.Reference}"));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return operation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Operation> RembourserAsync(int echeanceId, int utilisateurId, DateTime? maintenant = null)
    {
        var echeance = await _context.EcheancesPret.Include(e => e.Pret).ThenInclude(p => p.CompteDebit)
            .FirstOrDefaultAsync(e => e.Id == echeanceId)
            ?? throw new InvalidOperationException("Échéance introuvable.");
        var pret = echeance.Pret;
        var compte = pret.CompteDebit;
        var now = maintenant ?? DateTime.UtcNow;
        if (echeance.Statut == "Payee")
            throw new InvalidOperationException("Cette échéance est déjà payée.");
        if (pret.Statut is not ("EnCours" or "Impaye"))
            throw new InvalidOperationException("Ce prêt ne peut pas être remboursé dans son état actuel.");
        if (compte.Statut != "Actif")
            throw new InvalidOperationException("Le compte de remboursement est inactif.");

        var penalite = echeance.DateEcheance.Date < now.Date
            ? CalculerPenalite(echeance.MontantTotal, echeance.DateEcheance, now)
            : 0m;
        var montantDebit = echeance.MontantTotal + penalite;
        if (compte.Solde < montantDebit)
            throw new InvalidOperationException("Solde insuffisant pour rembourser cette échéance.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var operation = new Operation
            {
                Reference = GenererReference("REM"),
                TypeOperation = "RemboursementPret",
                CompteId = compte.Id,
                UtilisateurId = utilisateurId,
                AgenceId = compte.AgenceId,
                PretId = pret.Id,
                Montant = montantDebit,
                Devise = compte.Devise,
                Statut = "Valide",
                DateOperation = now,
                DateValidation = now,
                Commentaire = $"Remboursement échéance {echeance.NumeroEcheance} du prêt {pret.Reference}"
            };
            compte.Solde -= montantDebit;
            compte.DateModification = now;
            echeance.Penalite = penalite;
            echeance.DatePaiement = now;
            echeance.Statut = "Payee";
            pret.MontantRembourse += montantDebit;
            pret.SoldeRestant = Math.Max(0m, pret.SoldeRestant - echeance.Capital);
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();

            var ecritures = new List<Ecriture>
            {
                CreerEcriture(operation.Id, compte.Id, $"CLIENT:{compte.NumeroCompte}", "Debit", montantDebit, compte.Devise, operation.Commentaire!),
                CreerEcriture(operation.Id, null, $"PRETS_ENCOURS:{pret.Reference}", "Credit", echeance.Capital, compte.Devise, $"Capital remboursé - {pret.Reference}"),
                CreerEcriture(operation.Id, null, $"PRODUITS_INTERETS:{compte.AgenceId}", "Credit", echeance.Interet, compte.Devise, $"Intérêts du prêt {pret.Reference}")
            };
            if (penalite > 0m)
                ecritures.Add(CreerEcriture(operation.Id, null, $"PRODUITS_PENALITES:{compte.AgenceId}", "Credit", penalite, compte.Devise, $"Pénalité de retard du prêt {pret.Reference}"));
            _context.Ecritures.AddRange(ecritures);

            var restantes = await _context.EcheancesPret.CountAsync(e => e.PretId == pret.Id && e.Statut != "Payee");
            if (restantes == 0)
                pret.Statut = "Solde";
            else
            {
                var retards = await _context.EcheancesPret.CountAsync(e => e.PretId == pret.Id && e.Statut == "EnRetard");
                pret.Statut = retards >= 3 ? "Impaye" : "EnCours";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return operation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> MettreAJourRetardsAsync(DateTime? maintenant = null)
    {
        var aujourdHui = (maintenant ?? BankTime.Today).Date;
        var enRetard = await _context.EcheancesPret.Include(e => e.Pret)
            .Where(e => e.Statut != "Payee" && e.DateEcheance < aujourdHui)
            .ToListAsync();
        foreach (var echeance in enRetard)
        {
            echeance.Statut = "EnRetard";
            echeance.Penalite = CalculerPenalite(echeance.MontantTotal, echeance.DateEcheance, aujourdHui);
        }

        var parPret = enRetard.GroupBy(e => e.Pret);
        foreach (var groupe in parPret)
            groupe.Key.Statut = groupe.Count() >= 3 ? "Impaye" : "EnCours";

        await _context.SaveChangesAsync();
        return enRetard.Count;
    }

    public static decimal CalculerPenalite(decimal montantEcheance, DateTime dateEcheance, DateTime datePaiement)
    {
        var jours = (datePaiement.Date - dateEcheance.Date).Days;
        if (jours <= 0) return 0m;
        var mois = Math.Max(1, (int)Math.Ceiling(jours / 30m));
        var penalite = decimal.Round(montantEcheance * TauxPenaliteMensuel * mois, 2, MidpointRounding.AwayFromZero);
        return Math.Max(PenaliteMinimum, penalite);
    }

    private static Ecriture CreerEcriture(int operationId, int? compteId, string codeCompte, string sens, decimal montant, string devise, string libelle) => new()
    {
        OperationId = operationId,
        CompteId = compteId,
        CodeCompteGeneral = codeCompte,
        Sens = sens,
        Montant = montant,
        Devise = devise,
        DateEcriture = DateTime.UtcNow,
        Libelle = libelle
    };

    private static string GenererReference(string prefix) => $"{prefix}-{BankTime.Today:yyyyMMdd}-{RandomNumberGenerator.GetHexString(6)}";
}
