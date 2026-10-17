using System.Globalization;
using System.Security.Claims;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize]
public class RapportsController : Controller
{
    private const string StaffReportRoles = "admin,gestionnaire,auditeur";
    private readonly AkarangaBankContext _context;
    private readonly ICurrentUserService _currentUser;

    public RapportsController(AkarangaBankContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public IActionResult Index() => View();

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> AuditPdf()
    {
        var agences = await _context.Agences.AsNoTracking()
            .OrderBy(a => a.Code)
            .ToListAsync();
        var clients = await _context.Clients.AsNoTracking()
            .Include(c => c.Agence)
            .Include(c => c.Comptes).ThenInclude(c => c.TypeCompte)
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .ToListAsync();
        var comptes = clients.SelectMany(c => c.Comptes).ToList();
        var lignes = new List<string>
        {
            "AKARANGABANK - RAPPORT D'AUDIT",
            $"Genere le {BankTime.ToLocal(DateTime.UtcNow):dd/MM/yyyy HH:mm}",
            "",
            "RESUME",
            $"Agences : {agences.Count}",
            $"Clients : {clients.Count}",
            $"Comptes : {comptes.Count}",
            "Soldes totaux par devise :"
        };

        foreach (var devise in comptes.GroupBy(c => c.Devise).OrderBy(g => g.Key))
            lignes.Add($"  {devise.Key} : {devise.Sum(c => c.Solde):N2}");

        lignes.Add("");
        lignes.Add("LISTE DES AGENCES");
        foreach (var agence in agences)
        {
            var nombreClients = clients.Count(c => c.AgenceId == agence.Id);
            var nombreComptes = comptes.Count(c => c.AgenceId == agence.Id);
            lignes.Add($"{agence.Code} - {agence.Nom} ({agence.Ville}) | " +
                $"{(agence.EstSiege ? "Siege" : "Agence")} | Clients : {nombreClients} | Comptes : {nombreComptes}");
        }

        lignes.Add("");
        lignes.Add("CLIENTS ET LEURS COMPTES");
        foreach (var client in clients)
        {
            lignes.Add($"{client.NumeroClient} - {client.Prenom} {client.Nom} | " +
                $"Agence : {client.Agence?.Code ?? "-"} | Statut : {client.Statut}");
            foreach (var compte in client.Comptes.OrderBy(c => c.NumeroCompte))
                lignes.Add($"  {compte.NumeroCompte} | {compte.TypeCompte?.Libelle ?? "Type non renseigne"} | " +
                    $"{compte.Solde:N2} {compte.Devise} | {compte.Statut}");
        }

        return await PdfAsync(PdfReportBuilder.BuildAuditReport(lignes), "rapport-audit", "GenerationRapportAudit", "Rapports");
    }

    [HttpGet]
    public async Task<IActionResult> RelevePdf(int id)
    {
        var compte = await ChargerCompteAsync(id);
        if (compte is null)
            return NotFound();
        if (!await PeutConsulterCompteAsync(compte))
            return Forbid();

        var ecritures = await _context.Ecritures.AsNoTracking()
            .Include(e => e.Operation)
            .Where(e => e.CompteId == id && e.Operation.Statut == "Valide")
            .OrderBy(e => e.DateEcriture)
            .ToListAsync();
        var credits = ecritures.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
        var debits = ecritures.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
        var entries = ecritures.Select(e => new StatementPdfEntry(
            e.DateEcriture,
            e.Operation.Reference,
            e.Libelle ?? e.Operation.TypeOperation,
            e.Sens == "Debit" ? e.Montant : 0m,
            e.Sens == "Credit" ? e.Montant : 0m)).ToList();
        var pdf = PdfReportBuilder.BuildAccountStatement(
            $"{compte.Client.Prenom} {compte.Client.Nom}",
            compte.Client.NumeroClient,
            compte.NumeroCompte,
            compte.TypeCompte?.Libelle ?? "-",
            compte.Agence?.Nom ?? "-",
            compte.Devise,
            compte.Solde - credits + debits,
            compte.Solde,
            DateTime.UtcNow,
            entries);
        return await PdfAsync(pdf, $"releve-{compte.NumeroCompte}", "GenerationReleveCompte", "Comptes", compte.Id);
    }

    [HttpGet]
    public async Task<IActionResult> Releve(int id, string format = "csv")
    {
        var compte = await ChargerCompteAsync(id);
        if (compte is null)
            return NotFound();
        if (!await PeutConsulterCompteAsync(compte))
            return Forbid();

        var entries = await _context.Ecritures.AsNoTracking()
            .Include(e => e.Operation)
            .Where(e => e.CompteId == id && e.Operation.Statut == "Valide")
            .OrderBy(e => e.DateEcriture)
            .ToListAsync();
        var table = new SpreadsheetTable(
            ["Date", "Référence opération", "Libellé", "Débit", "Crédit", "Devise"],
            entries.Select(e => (IReadOnlyList<object?>)[
                BankTime.ToLocal(e.DateEcriture).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                e.Operation.Reference,
                e.Libelle ?? e.Operation.TypeOperation,
                e.Sens == "Debit" ? e.Montant : 0m,
                e.Sens == "Credit" ? e.Montant : 0m,
                e.Devise
            ]).ToList());
        return await ExportAsync(table, $"releve-{compte.NumeroCompte}", $"Relevé de compte {compte.NumeroCompte}", format, "ExportReleveCompte", "Comptes", compte.Id);
    }

    [HttpGet]
    public async Task<IActionResult> AttestationSoldePdf(int id)
    {
        var compte = await ChargerCompteAsync(id);
        if (compte is null)
            return NotFound();
        if (!await PeutConsulterCompteAsync(compte))
            return Forbid();

        var pdf = PdfReportBuilder.BuildBalanceCertificate(
            $"{compte.Client.Prenom} {compte.Client.Nom}",
            compte.Client.NumeroClient,
            compte.NumeroCompte,
            compte.TypeCompte?.Libelle ?? "-",
            compte.Agence?.Nom ?? "-",
            compte.Solde,
            compte.Devise,
            DateTime.UtcNow);
        return await PdfAsync(pdf, $"attestation-solde-{compte.NumeroCompte}", "GenerationAttestationSolde", "Comptes", compte.Id);
    }

    [HttpGet]
    public async Task<IActionResult> ContratPretPdf(int id)
    {
        var pret = await _context.Prets.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.TypePret)
            .Include(p => p.CompteDebit)
            .Include(p => p.Echeances.OrderBy(e => e.NumeroEcheance))
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pret is null)
            return NotFound();
        if (!await PeutConsulterPretAsync(pret))
            return Forbid();
        if (!pret.DateValidation.HasValue || pret.Statut == "Rejete")
            return Conflict("Le contrat PDF est disponible après validation du prêt.");

        var pdf = PdfReportBuilder.BuildLoanContract(
            pret.Reference,
            $"{pret.Client.Prenom} {pret.Client.Nom}",
            pret.Client.NumeroClient,
            pret.Client.NumeroCNI,
            pret.TypePret.Libelle,
            pret.CompteDebit.NumeroCompte,
            pret.MontantPrincipal,
            pret.CompteDebit.Devise,
            pret.TauxInteret,
            pret.DureeMois,
            pret.Mensualite,
            pret.Commentaire ?? string.Empty,
            pret.DateValidation.Value,
            pret.Echeances.OrderBy(e => e.NumeroEcheance).Select(e => new LoanSchedulePdfEntry(
                e.NumeroEcheance,
                e.DateEcheance,
                e.Capital,
                e.Interet,
                e.MontantTotal,
                e.Statut)).ToList());
        return await PdfAsync(pdf, $"contrat-pret-{pret.Reference}", "GenerationContratPret", "Prets", pret.Id);
    }

    [HttpGet]
    public async Task<IActionResult> RecuOperationPdf(int id)
    {
        var operation = await _context.Operations.AsNoTracking()
            .Include(o => o.Compte).ThenInclude(c => c.Client)
            .Include(o => o.Ecritures).ThenInclude(e => e.Compte)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null)
            return NotFound();
        if (!await PeutConsulterCompteAsync(operation.Compte))
            return Forbid();
        if (operation.Statut != "Valide")
            return Conflict("Le reçu définitif est disponible après validation de l'opération.");

        var entries = operation.Ecritures.OrderBy(e => e.Id)
            .Select(e => (
                e.Sens,
                e.Compte?.NumeroCompte ?? e.CodeCompteGeneral ?? "—",
                e.Libelle ?? operation.TypeOperation,
                e.Montant))
            .ToList();
        var pdf = PdfReportBuilder.BuildOperationReceipt(
            operation.Reference,
            operation.TypeOperation,
            operation.Statut,
            $"{operation.Compte.Client.Prenom} {operation.Compte.Client.Nom}",
            operation.Compte.NumeroCompte,
            operation.Montant,
            operation.Devise,
            operation.DateOperation,
            entries);
        return await PdfAsync(pdf, $"recu-{operation.Reference}", "GenerationRecuOperation", "Operations", operation.Id);
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> Journal(DateOnly? du, DateOnly? au, string? typeOperation, string format = "html")
    {
        if (!TryGetDateRange(du, au, out var range, out var error))
            return BadRequest(error);
        var entries = await GetEntriesAsync(range.StartUtc, range.EndUtc, typeOperation);
        var table = new SpreadsheetTable(
            ["Date", "Référence opération", "Type", "Compte général", "Libellé", "Sens", "Montant", "Devise"],
            entries.Select(e => (IReadOnlyList<object?>)[
                BankTime.ToLocal(e.DateEcriture).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                e.Operation.Reference,
                e.Operation.TypeOperation,
                e.CodeCompteGeneral ?? "NON_CLASSE",
                e.Libelle ?? e.Operation.TypeOperation,
                e.Sens,
                e.Montant,
                e.Devise
            ]).ToList());
        return await ExportAsync(table, "journal-comptable", "Journal comptable", format, "ExportJournalComptable", "Rapports", filters: new { du, au, typeOperation });
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> BalanceGenerale(DateOnly? au, string format = "html")
    {
        var endDate = au ?? DateOnly.FromDateTime(BankTime.Today);
        if (endDate == DateOnly.MaxValue)
            return BadRequest("La date de clôture demandée est invalide.");
        var entries = await GetEntriesAsync(null, ToUtc(endDate.AddDays(1)));
        var rows = entries
            .GroupBy(e => new { Compte = e.CodeCompteGeneral ?? "NON_CLASSE", e.Devise })
            .OrderBy(g => g.Key.Compte).ThenBy(g => g.Key.Devise)
            .Select(g =>
            {
                var debit = g.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
                var credit = g.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
                return (IReadOnlyList<object?>)[g.Key.Compte, g.Key.Devise, debit, credit, debit - credit, ""];
            }).ToList();
        foreach (var devise in entries.GroupBy(e => e.Devise))
        {
            var debit = devise.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
            var credit = devise.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
            rows.Add(["TOTAL GÉNÉRAL", devise.Key, debit, credit, debit - credit, debit == credit ? "Équilibrée" : "Écart à vérifier"]);
        }
        var table = new SpreadsheetTable(
            ["Compte général", "Devise", "Total débits", "Total crédits", "Solde débiteur (débit - crédit)", "Contrôle"],
            rows);
        return await ExportAsync(table, $"balance-generale-au-{endDate:yyyyMMdd}", "Balance générale", format, "ExportBalanceGenerale", "Rapports", filters: new { au = endDate });
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> CompteResultat(DateOnly? du, DateOnly? au, string format = "html")
    {
        if (!TryGetDateRange(du, au, out var range, out var error))
            return BadRequest(error);
        var entries = await GetEntriesAsync(range.StartUtc, range.EndUtc);
        var rows = entries
            .Where(e => IsCompteResultat(e.CodeCompteGeneral))
            .GroupBy(e => new { Compte = e.CodeCompteGeneral ?? "NON_CLASSE", e.Devise })
            .OrderBy(g => g.Key.Compte).ThenBy(g => g.Key.Devise)
            .Select(g =>
            {
                var debit = g.Where(e => e.Sens == "Debit").Sum(e => e.Montant);
                var credit = g.Where(e => e.Sens == "Credit").Sum(e => e.Montant);
                return (IReadOnlyList<object?>)[
                    g.Key.Compte, g.Key.Devise, debit, credit,
                    g.Key.Compte.StartsWith("PRODUITS_", StringComparison.OrdinalIgnoreCase)
                        ? credit - debit
                        : debit - credit
                ];
            }).ToList();
        foreach (var devise in entries.Where(e => IsCompteResultat(e.CodeCompteGeneral)).GroupBy(e => e.Devise))
        {
            var produits = devise.Where(e => e.CodeCompteGeneral!.StartsWith("PRODUITS_", StringComparison.OrdinalIgnoreCase))
                .Sum(e => e.Sens == "Credit" ? e.Montant : -e.Montant);
            var charges = devise.Where(e => e.CodeCompteGeneral!.StartsWith("CHARGES_", StringComparison.OrdinalIgnoreCase))
                .Sum(e => e.Sens == "Debit" ? e.Montant : -e.Montant);
            rows.Add(["TOTAL PRODUITS", devise.Key, 0m, produits, produits]);
            rows.Add(["TOTAL CHARGES", devise.Key, charges, 0m, charges]);
            rows.Add(["RÉSULTAT NET", devise.Key, charges, produits, produits - charges]);
        }
        var table = new SpreadsheetTable(
            ["Compte / synthèse", "Devise", "Débits", "Crédits", "Produit / charge nette"],
            rows);
        return await ExportAsync(table, $"compte-resultat-{range.Start:yyyyMMdd}-{range.End:yyyyMMdd}", "Compte de résultat", format, "ExportCompteResultat", "Rapports", filters: new { du = range.Start, au = range.End });
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> Bilan(DateOnly? au, string format = "html")
    {
        var endDate = au ?? DateOnly.FromDateTime(BankTime.Today);
        if (endDate == DateOnly.MaxValue)
            return BadRequest("La date de clôture demandée est invalide.");
        var entries = await GetEntriesAsync(null, ToUtc(endDate.AddDays(1)));
        var balances = entries
            .GroupBy(e => new { Compte = e.CodeCompteGeneral ?? "NON_CLASSE", e.Devise })
            .Select(g => new
            {
                g.Key.Compte,
                g.Key.Devise,
                SoldeDebit = g.Where(e => e.Sens == "Debit").Sum(e => e.Montant)
                    - g.Where(e => e.Sens == "Credit").Sum(e => e.Montant)
            })
            .ToList();

        var rows = new List<IReadOnlyList<object?>>();
        foreach (var solde in balances.OrderBy(x => x.Compte).ThenBy(x => x.Devise))
        {
            var (rubrique, montant) = ClasserSoldeBilan(solde.Compte, solde.SoldeDebit);
            if (montant != 0m)
                rows.Add([rubrique, solde.Compte, solde.Devise, montant]);
        }
        var totals = rows
            .GroupBy(row => (string)row[0]!)
            .Select(g => new { Rubrique = g.Key, Devise = (string)g.First()[2]!, Total = g.Sum(row => (decimal)row[3]!) })
            .OrderBy(g => g.Rubrique).ThenBy(g => g.Devise)
            .ToList();
        foreach (var total in totals)
            rows.Add([$"TOTAL {total.Rubrique}", "", total.Devise, total.Total]);

        var balanceByCurrency = rows.Where(row => !((string)row[0]!).StartsWith("TOTAL ", StringComparison.Ordinal))
            .GroupBy(row => (string)row[2]!)
            .ToList();
        foreach (var devise in balanceByCurrency)
        {
            var actifs = devise.Where(row => ((string)row[0]!).StartsWith("ACTIF", StringComparison.Ordinal))
                .Sum(row => (decimal)row[3]!);
            var passifs = devise.Where(row =>
                    ((string)row[0]!).StartsWith("PASSIF", StringComparison.Ordinal)
                    || ((string)row[0]!).StartsWith("DETTES", StringComparison.Ordinal))
                .Sum(row => (decimal)row[3]!);
            var capitaux = devise.Where(row => ((string)row[0]!).StartsWith("CAPITAUX PROPRES", StringComparison.Ordinal))
                .Sum(row => (decimal)row[3]!);
            rows.Add(["TOTAL ACTIF", "", devise.Key, actifs]);
            rows.Add(["TOTAL PASSIF", "", devise.Key, passifs]);
            rows.Add(["TOTAL CAPITAUX PROPRES", "", devise.Key, capitaux]);
            rows.Add(["CONTRÔLE ACTIF = PASSIF + CAPITAUX PROPRES", "", devise.Key, actifs - passifs - capitaux]);
        }

        var table = new SpreadsheetTable(
            ["Rubrique du bilan", "Compte général", "Devise", "Solde normal"],
            rows);
        return await ExportAsync(table, $"bilan-au-{endDate:yyyyMMdd}", "Bilan", format, "ExportBilan", "Rapports", filters: new { au = endDate });
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> RapportAmlMensuel(int? annee, int? mois, string format = "html")
    {
        var currentDate = DateOnly.FromDateTime(BankTime.Today);
        var year = annee ?? currentDate.Year;
        var month = mois ?? currentDate.Month;
        if (year is < 2000 or > 9999 || month is < 1 or > 12)
            return BadRequest("Le mois ou l’année du rapport AML est invalide.");

        DateOnly start;
        try
        {
            start = new DateOnly(year, month, 1);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest("Le mois ou l’année du rapport AML est invalide.");
        }
        if (start == new DateOnly(9999, 12, 1))
            return BadRequest("La période demandée est trop éloignée.");
        var startUtc = ToUtc(start);
        var endUtc = ToUtc(start.AddMonths(1));
        var alerts = await _context.AlertesAML.AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Operation)
            .Where(a => a.DateAlerte >= startUtc && a.DateAlerte < endUtc)
            .OrderBy(a => a.DateAlerte)
            .ToListAsync();
        var dosTransmises = await _context.DeclarationsSoupcon.AsNoTracking()
            .CountAsync(d => d.TypeDeclaration == "DOS"
                && d.DateTransmission >= startUtc && d.DateTransmission < endUtc);
        var operationsSuspectes = alerts.Where(a => a.OperationId.HasValue && a.Operation is not null)
            .GroupBy(a => a.OperationId!.Value)
            .Select(group => group.First().Operation!)
            .ToList();
        var rows = new List<IReadOnlyList<object?>>();
        rows.Add(["SYNTHÈSE", "Nombre total d'alertes", "", alerts.Count, "", "", "", "", ""]);
        foreach (var niveau in alerts.GroupBy(a => a.Niveau).OrderByDescending(group => PrioriteNiveau(group.Key)))
            rows.Add(["SYNTHÈSE", "Alertes par niveau", niveau.Key, niveau.Count(), "", "", "", "", ""]);
        rows.Add(["SYNTHÈSE", "DOS transmises", "", dosTransmises, "", "", "", "", ""]);
        foreach (var devise in operationsSuspectes.GroupBy(o => o.Devise).OrderBy(group => group.Key))
            rows.Add(["SYNTHÈSE", "Montant total des opérations suspectes", "", operationsSuspectes.Count(o => o.Devise == devise.Key), devise.Sum(o => o.Montant), devise.Key, "", "", ""]);

        rows.Add(["TOP 10 CLIENTS", "Rang", "Niveau de risque", "Nombre d'alertes", "Montant suspect", "Devise", "Client", "Numéro client", ""]);
        var topClients = alerts.GroupBy(a => new { a.ClientId, a.Client.NumeroClient, a.Client.Nom, a.Client.Prenom })
            .Select(group => new
            {
                group.Key.ClientId,
                group.Key.NumeroClient,
                Nom = $"{group.Key.Prenom} {group.Key.Nom}",
                NombreAlertes = group.Count(),
                Niveau = group.OrderByDescending(a => PrioriteNiveau(a.Niveau)).First().Niveau,
                Operations = group.Where(a => a.OperationId.HasValue && a.Operation is not null)
                    .GroupBy(a => a.OperationId!.Value).Select(operation => operation.First().Operation!).ToList()
            })
            .OrderByDescending(client => PrioriteNiveau(client.Niveau))
            .ThenByDescending(client => client.NombreAlertes)
            .Take(10)
            .ToList();
        for (var index = 0; index < topClients.Count; index++)
        {
            var client = topClients[index];
            var devises = string.Join(", ", client.Operations.GroupBy(o => o.Devise)
                .Select(group => $"{group.Sum(o => o.Montant):N2} {group.Key}"));
            rows.Add(["TOP 10 CLIENTS", index + 1, client.Niveau, client.NombreAlertes, devises, "", client.Nom, client.NumeroClient, ""]);
        }

        rows.Add(["DÉTAIL DES ALERTES", "Id", "Niveau", "Montant", "Devise", "Date", "Client", "Référence opération", "Type / statut / description"]);
        foreach (var alerte in alerts)
        {
            rows.Add(["DÉTAIL DES ALERTES", alerte.Id, alerte.Niveau, alerte.Operation?.Montant ?? 0m,
                alerte.Operation?.Devise ?? "", BankTime.ToLocal(alerte.DateAlerte).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                $"{alerte.Client.Prenom} {alerte.Client.Nom}", alerte.Operation?.Reference ?? "",
                $"{alerte.TypeAlerte} · {alerte.Statut} · {alerte.Description}"]);
        }
        var table = new SpreadsheetTable(
            ["Section", "Indicateur / rang", "Niveau", "Nombre", "Montant", "Devise", "Client", "Numéro / référence", "Détail"],
            rows);
        return await ExportAsync(table, $"rapport-aml-{year}{month:00}", $"Rapport AML — {year}-{month:00}", format, "ExportRapportAmlMensuel", "AlertesAML", filters: new { annee = year, mois = month });
    }

    [HttpGet]
    [Authorize(Roles = StaffReportRoles)]
    public async Task<IActionResult> PortefeuillePrets(string format = "html")
    {
        var prets = await _context.Prets.AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.TypePret)
            .Include(p => p.CompteDebit)
            .OrderByDescending(p => p.DateDemande)
            .ToListAsync();
        var table = new SpreadsheetTable(
            ["Référence", "Client", "Numéro client", "Produit", "Principal", "Solde restant", "Devise", "Taux annuel", "Durée (mois)", "Mensualité", "Compte de remboursement", "Statut", "Date de demande"],
            prets.Select(p => (IReadOnlyList<object?>)[
                p.Reference,
                $"{p.Client.Prenom} {p.Client.Nom}",
                p.Client.NumeroClient,
                p.TypePret.Libelle,
                p.MontantPrincipal,
                p.SoldeRestant,
                p.CompteDebit.Devise,
                p.TauxInteret,
                p.DureeMois,
                p.Mensualite,
                p.CompteDebit.NumeroCompte,
                p.Statut,
                BankTime.ToLocal(p.DateDemande)
            ]).ToList());
        return await ExportAsync(table, "portefeuille-prets", "Portefeuille de prêts", format, "ExportPortefeuillePrets", "Prets");
    }

    private async Task<List<Ecriture>> GetEntriesAsync(DateTime? startUtc, DateTime endUtc, string? typeOperation = null) =>
        await _context.Ecritures.AsNoTracking()
            .Include(e => e.Operation)
            .Where(e => e.Operation.Statut == "Valide"
                && e.DateEcriture < endUtc
                && (!startUtc.HasValue || e.DateEcriture >= startUtc.Value)
                && (string.IsNullOrWhiteSpace(typeOperation) || e.Operation.TypeOperation == typeOperation))
            .OrderBy(e => e.DateEcriture)
            .ThenBy(e => e.Id)
            .ToListAsync();

    private Task<Compte?> ChargerCompteAsync(int id) => _context.Comptes.AsNoTracking()
        .Include(c => c.Client)
        .Include(c => c.TypeCompte)
        .Include(c => c.Agence)
        .FirstOrDefaultAsync(c => c.Id == id);

    private async Task<bool> PeutConsulterCompteAsync(Compte compte)
    {
        if (User.IsInRole("client"))
            return await GetClientIdAsync() == compte.ClientId;
        return User.IsInRole("admin") || User.IsInRole("gestionnaire")
            || User.IsInRole("guichetier") || User.IsInRole("auditeur");
    }

    private async Task<bool> PeutConsulterPretAsync(Pret pret)
    {
        if (User.IsInRole("client"))
            return await GetClientIdAsync() == pret.ClientId;
        return User.IsInRole("admin") || User.IsInRole("gestionnaire")
            || User.IsInRole("guichetier") || User.IsInRole("auditeur");
    }

    private async Task<int?> GetClientIdAsync()
    {
        var userId = await _currentUser.GetUserIdAsync();
        return await _context.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.ClientId)
            .FirstOrDefaultAsync();
    }

    private bool TryGetDateRange(
        DateOnly? from,
        DateOnly? through,
        out ReportDateRange range,
        out string error)
    {
        var localToday = DateOnly.FromDateTime(BankTime.Today);
        var endDate = through ?? localToday;
        var startDate = from ?? new DateOnly(endDate.Year, endDate.Month, 1);
        if (startDate > endDate || endDate == DateOnly.MaxValue)
        {
            range = default;
            error = endDate == DateOnly.MaxValue
                ? "La date de fin demandée est invalide."
                : "La date de début doit précéder ou égaler la date de fin.";
            return false;
        }

        range = new ReportDateRange(startDate, endDate, ToUtc(startDate), ToUtc(endDate.AddDays(1)));
        error = string.Empty;
        return true;
    }

    private static DateTime ToUtc(DateOnly localDate)
    {
        var local = DateTime.SpecifyKind(localDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, BankTime.TimeZone);
    }

    private async Task<IActionResult> ExportAsync(
        SpreadsheetTable table,
        string name,
        string title,
        string format,
        string auditAction,
        string auditTable,
        int? recordId = null,
        object? filters = null)
    {
        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var file = SpreadsheetExport.BuildXlsx(table);
            await EnregistrerAuditAsync(auditAction, auditTable, recordId, $"Export Excel : {name}.");
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{name}.xlsx");
        }
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var file = SpreadsheetExport.BuildCsv(table);
            await EnregistrerAuditAsync(auditAction, auditTable, recordId, $"Export CSV : {name}.");
            return File(file, "text/csv; charset=utf-8", $"{name}.csv");
        }
        if (string.Equals(format, "html", StringComparison.OrdinalIgnoreCase))
        {
            await EnregistrerAuditAsync("ConsultationRapport", auditTable, recordId, $"Consultation : {name}.");
            return View("Table", new ReportTableViewModel(
                title,
                RouteData.Values["action"]?.ToString() ?? "Index",
                table,
                CreateRouteValues(filters)));
        }
        return BadRequest("Format d’export non pris en charge. Utilisez html, csv ou xlsx.");
    }

    private async Task<IActionResult> PdfAsync(
        byte[] pdf,
        string fileName,
        string auditAction,
        string auditTable,
        int? recordId = null)
    {
        var timestamp = BankTime.ToLocal(DateTime.UtcNow).ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        await EnregistrerAuditAsync(auditAction, auditTable, recordId, $"Document PDF généré : {fileName}.");
        return File(pdf, "application/pdf", $"{fileName}-{timestamp}.pdf");
    }

    private async Task EnregistrerAuditAsync(string action, string table, int? recordId, string details)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : (int?)null;
        var userAgent = Request.Headers.UserAgent.ToString();
        _context.AuditLogs.Add(new AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = userId,
            Action = action,
            TableConcernee = table,
            EnregistrementId = recordId,
            AdresseIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = userAgent.Length <= 255 ? userAgent : userAgent[..255],
            Details = details
        });
        await _context.SaveChangesAsync();
    }

    private Dictionary<string, string> CreateRouteValues(object? filters)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (filters is null)
            return values;
        foreach (var property in filters.GetType().GetProperties())
        {
            var value = property.GetValue(filters);
            if (value is null)
                continue;
            var text = value switch
            {
                DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
            if (!string.IsNullOrWhiteSpace(text))
                values[property.Name] = text;
        }
        return values;
    }

    private static int PrioriteNiveau(string? niveau) => niveau?.ToLowerInvariant() switch
    {
        "critique" or "critical" => 4,
        "eleve" or "élevé" or "high" => 3,
        "moyen" or "medium" => 2,
        "faible" or "low" => 1,
        _ => 0
    };

    private static bool IsCompteResultat(string? accountCode) =>
        accountCode?.StartsWith("PRODUITS_", StringComparison.OrdinalIgnoreCase) == true
        || accountCode?.StartsWith("CHARGES_", StringComparison.OrdinalIgnoreCase) == true;

    private static (string Rubrique, decimal Montant) ClasserSoldeBilan(string code, decimal soldeDebit)
    {
        if (code.StartsWith("CAISSE:", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("PRETS_ENCOURS:", StringComparison.OrdinalIgnoreCase))
            return soldeDebit >= 0 ? ("ACTIF", soldeDebit) : ("PASSIF", -soldeDebit);
        if (code.StartsWith("COMPENSATION:", StringComparison.OrdinalIgnoreCase)
            || code.StartsWith("COMPENSATION_CARTES:", StringComparison.OrdinalIgnoreCase))
            return soldeDebit >= 0
                ? ("ACTIF - COMPENSATION", soldeDebit)
                : ("PASSIF - COMPENSATION", -soldeDebit);
        if (code.StartsWith("CLIENT:", StringComparison.OrdinalIgnoreCase))
            return soldeDebit <= 0
                ? ("DETTES ENVERS LES CLIENTS", -soldeDebit)
                : ("ACTIF - CREANCES CLIENTS", soldeDebit);
        if (code.StartsWith("PRODUITS_", StringComparison.OrdinalIgnoreCase))
            return ("CAPITAUX PROPRES - PRODUITS", -soldeDebit);
        if (code.StartsWith("CHARGES_", StringComparison.OrdinalIgnoreCase))
            return ("CAPITAUX PROPRES - CHARGES", -soldeDebit);
        if (code.StartsWith("REGLEMENT:", StringComparison.OrdinalIgnoreCase))
            return soldeDebit >= 0
                ? ("ACTIF - REGLEMENT", soldeDebit)
                : ("PASSIF - REGLEMENT", -soldeDebit);
        return soldeDebit >= 0
            ? ("AUTRES ACTIFS", soldeDebit)
            : ("AUTRES PASSIFS", -soldeDebit);
    }

    private readonly record struct ReportDateRange(
        DateOnly Start,
        DateOnly End,
        DateTime StartUtc,
        DateTime EndUtc);
}
