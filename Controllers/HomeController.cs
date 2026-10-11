using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class HomeController : Controller
{
    private readonly AkarangaBankContext _context;

    public HomeController(AkarangaBankContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var soldes = await _context.Comptes.AsNoTracking()
            .Where(c => !c.NumeroCompte.StartsWith("9000"))
            .GroupBy(c => c.Devise)
            .Select(g => new { Devise = g.Key, Montant = g.Sum(c => c.Solde) })
            .OrderBy(x => x.Devise)
            .ToListAsync();
        var soldesParDevise = soldes.Select(x => new DepotParDevise(x.Devise, x.Montant)).ToList();

        var virementsEnAttente = await _context.VirementsInterAgences.AsNoTracking()
            .Where(v => v.Statut == "EnAttente")
            .Include(v => v.AgenceSource)
            .Include(v => v.AgenceDestination)
            .OrderByDescending(v => v.DateCreation)
            .Take(6)
            .ToListAsync();

        var model = new AdminDashboardViewModel(
            await _context.Clients.CountAsync(),
            await _context.Agences.CountAsync(),
            soldesParDevise,
            await _context.VirementsInterAgences.CountAsync(v => v.Statut == "EnAttente"),
            virementsEnAttente);

        return View(model);
    }

    public IActionResult Settings() => View();
}

public sealed record DepotParDevise(string Devise, decimal Montant);

public sealed record AdminDashboardViewModel(
    int NombreClients,
    int NombreAgences,
    IReadOnlyList<DepotParDevise> DepotsParDevise,
    int VirementsEnAttente,
    IReadOnlyList<VirementInterAgence> DerniersVirementsEnAttente);
