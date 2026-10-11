using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = BankAuthorizationPolicies.GestionnaireOrAdmin)]
public class ConformiteController : Controller
{
    private readonly IDeclarationService _declarationService;
    private readonly IConformiteService _conformiteService;
    private readonly ICurrentUserService _currentUser;

    public ConformiteController(
        IDeclarationService declarationService,
        IConformiteService conformiteService,
        ICurrentUserService currentUser)
    {
        _declarationService = declarationService;
        _conformiteService = conformiteService;
        _currentUser = currentUser;
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
        return File(AmlCsvExport.Build(alertes), "text/csv; charset=utf-8", fileName);
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
}
