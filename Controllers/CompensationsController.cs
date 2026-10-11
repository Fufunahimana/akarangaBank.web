using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class CompensationsController : Controller
{
    private readonly ICompensationService _service;
    private readonly AkarangaBankContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompensationsController(ICompensationService service, AkarangaBankContext context, ICurrentUserService currentUser)
    {
        _service = service;
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(int? agenceId, DateTime? date)
    {
        var jour = (date ?? BankTime.Today).Date;
        ViewBag.Agences = new SelectList(await _context.Agences.AsNoTracking().OrderBy(a => a.Code).ToListAsync(), "Id", "Code", agenceId);
        ViewBag.AgenceId = agenceId;
        ViewBag.Date = jour;
        ViewBag.Equilibree = await _service.VerifierEquilibreCompensationsAsync(jour);
        var toutesLesCompensations = await _service.GetCompensationsAsync(null, jour);
        ViewBag.TotalDebitsJour = toutesLesCompensations.Sum(c => c.TotalDebits);
        ViewBag.TotalCreditsJour = toutesLesCompensations.Sum(c => c.TotalCredits);
        var compensationsAffichees = agenceId.HasValue
            ? toutesLesCompensations.Where(c => c.AgenceId == agenceId.Value)
            : toutesLesCompensations;
        return View(compensationsAffichees);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Calculer(int agenceId, DateTime date)
    {
        try
        {
            var resultat = await _service.CalculerCompensationAsync(agenceId, date);
            TempData["Success"] = $"Compensation calculée pour {resultat.DateCompensation:dd/MM/yyyy} : débits {resultat.TotalDebits:N2}, crédits {resultat.TotalCredits:N2} BIF.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { agenceId, date = date.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regler(int id)
    {
        try
        {
            var ok = await _service.ReglerCompensationAsync(id, await _currentUser.GetUserIdAsync());
            TempData[ok ? "Success" : "Error"] = ok ? "Compensation réglée." : "Compensation introuvable.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
