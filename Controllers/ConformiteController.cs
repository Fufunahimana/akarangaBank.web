using AkarangaBank.Web.Services;
using AkarangaBank.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = BankAuthorizationPolicies.GestionnaireOrAdmin)]
public class ConformiteController : Controller
{
    private readonly IDeclarationService _declarationService;
    private readonly IConformiteService _conformiteService;
    private readonly ICurrentUserService _currentUser;
    private readonly AkarangaBankContext _context;

    public ConformiteController(
        IDeclarationService declarationService,
        IConformiteService conformiteService,
        ICurrentUserService currentUser,
        AkarangaBankContext context)
    {
        _declarationService = declarationService;
        _conformiteService = conformiteService;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<IActionResult> Alertes(string? statut, string? niveau)
    {
        var alertes = await _declarationService.GetAlertesAsync(statut, niveau);
        ViewBag.Statut = statut;
        ViewBag.Niveau = niveau;
        return View(alertes);
    }

    [HttpGet]
    public async Task<IActionResult> ExporterCsv(string? statut, string? niveau)
    {
        var alertes = await _declarationService.GetAlertesAsync(statut, niveau);
        var fileName = $"alertes-aml-{BankTime.ToLocal(DateTime.UtcNow):yyyyMMdd-HHmmss}.csv";
        var file = AmlCsvExport.Build(alertes);
        await EnregistrerExportAuditAsync("ExportAlertesAmlCsv", statut, niveau);
        return File(file, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExporterExcel(string? statut, string? niveau)
    {
        var alertes = (await _declarationService.GetAlertesAsync(statut, niveau)).ToList();
        var table = new SpreadsheetTable(
            ["Id alerte", "Date locale", "Numéro client", "Nom du client", "Type", "Niveau", "Statut", "Référence opération", "Description"],
            alertes.Select(alert => (IReadOnlyList<object?>)[
                alert.Id,
                BankTime.ToLocal(alert.DateAlerte),
                alert.Client.NumeroClient,
                $"{alert.Client.Prenom} {alert.Client.Nom}",
                alert.TypeAlerte,
                alert.Niveau,
                alert.Statut,
                alert.Operation?.Reference ?? "",
                alert.Description ?? ""
            ]).ToList());
        var fileName = $"alertes-aml-{BankTime.ToLocal(DateTime.UtcNow):yyyyMMdd-HHmmss}.xlsx";
        var file = SpreadsheetExport.BuildXlsx(table);
        await EnregistrerExportAuditAsync("ExportAlertesAmlExcel", statut, niveau);
        return File(file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int id)
    {
        var alerte = (await _declarationService.GetAlertesAsync(null, null)).FirstOrDefault(a => a.Id == id);
        return alerte is null ? NotFound() : View(alerte);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Traiter(int id, string statut, string? commentaire)
    {
        if (statut is not ("EnCours" or "Traitee" or "Classee"))
            return BadRequest("Statut d'alerte invalide.");

        var userId = await _currentUser.GetUserIdAsync();
        var traite = await _declarationService.TraiterAlerteAsync(id, userId, statut, commentaire);
        if (!traite)
            return NotFound();

        TempData["Success"] = "Alerte traitée.";
        return RedirectToAction(nameof(Alertes));
    }

    public async Task<IActionResult> RisqueClient(int id)
    {
        ViewBag.ClientId = id;
        ViewBag.Risque = await _conformiteService.EvaluerRisqueClientAsync(id);
        return View();
    }

    private async Task EnregistrerExportAuditAsync(string action, string? statut, string? niveau)
    {
        var userId = await _currentUser.GetUserIdAsync();
        _context.AuditLogs.Add(new AkarangaBank.Web.Models.AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = userId,
            Action = action,
            TableConcernee = "AlertesAML",
            AdresseIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Details = $"Export des alertes AML. Statut={statut ?? "tous"}; niveau={niveau ?? "tous"}."
        });
        await _context.SaveChangesAsync();
    }
}
