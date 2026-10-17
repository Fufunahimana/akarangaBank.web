using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using AkarangaBank.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AkarangaBankContext _context;
    private readonly ICurrentUserService _currentUser;

    public HomeController(AkarangaBankContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("client"))
        {
            var userId = await _currentUser.GetUserIdAsync();
            var user = await _context.Users.AsNoTracking()
                .Include(u => u.Client)
                    .ThenInclude(c => c!.Agence)
                .FirstOrDefaultAsync(u => u.Id == userId);
            var clientId = user?.ClientId;
            var comptes = clientId.HasValue
                ? await _context.Comptes.AsNoTracking()
                    .Include(c => c.TypeCompte)
                    .Include(c => c.Agence)
                    .Where(c => c.ClientId == clientId.Value && c.Statut == "Actif")
                    .OrderBy(c => c.NumeroCompte)
                    .ToListAsync()
                : new List<Compte>();
            var compteIds = comptes.Select(c => c.Id).ToArray();
            var activite = compteIds.Length == 0
                ? new List<Ecriture>()
                : await _context.Ecritures.AsNoTracking()
                    .Include(e => e.Operation)
                    .Include(e => e.Compte)
                    .Where(e => e.CompteId.HasValue && compteIds.Contains(e.CompteId.Value))
                    .OrderByDescending(e => e.DateEcriture)
                    .Take(12)
                    .ToListAsync();
            var agences = await _context.Agences.AsNoTracking()
                .OrderBy(a => a.Ville).ThenBy(a => a.Nom)
                .ToListAsync();

            return View("Client", new EspaceClientViewModel
            {
                NomComplet = user?.NomComplet ?? string.Empty,
                Client = user?.Client,
                Comptes = comptes,
                ActiviteRecente = activite,
                Agences = agences
            });
        }

        if (!User.IsInRole("admin") && !User.IsInRole("gestionnaire"))
            return RedirectToAction("Index", "Clients");

        var soldes = await _context.Comptes.AsNoTracking()
            .Where(c => !c.NumeroCompte.StartsWith("9000"))
            .GroupBy(c => c.Devise)
            .Select(g => new { Devise = g.Key, Montant = g.Sum(c => c.Solde) })
            .OrderBy(x => x.Devise)
            .ToListAsync();
        var soldesParDevise = soldes.Select(x => new DepotParDevise(x.Devise, x.Montant)).ToList();
        var nombreComptes = await _context.Comptes.AsNoTracking()
            .CountAsync(c => !c.NumeroCompte.StartsWith("9000"));
        var (debutJourUtc, finJourUtc) = BankTime.GetUtcDayRange(BankTime.Today);
        var operationsDuJour = await _context.Operations.AsNoTracking()
            .CountAsync(o => o.DateOperation >= debutJourUtc && o.DateOperation < finJourUtc);
        var pretsEnCours = await _context.Prets.AsNoTracking()
            .CountAsync(p => p.Statut == "EnCours" || p.Statut == "Impaye");
        var alertesAmlOuvertes = await _context.AlertesAML.AsNoTracking()
            .CountAsync(a => a.Statut == "Ouverte" || a.Statut == "EnCours");

        var virementsEnAttente = await _context.VirementsInterAgences.AsNoTracking()
            .Where(v => v.Statut == "EnAttente")
            .Include(v => v.AgenceSource)
            .Include(v => v.AgenceDestination)
            .OrderByDescending(v => v.DateCreation)
            .Take(6)
            .ToListAsync();

        var model = new AdminDashboardViewModel(
            await _context.Clients.CountAsync(),
            nombreComptes,
            await _context.Agences.CountAsync(),
            soldesParDevise,
            operationsDuJour,
            pretsEnCours,
            alertesAmlOuvertes,
            await _context.VirementsInterAgences.CountAsync(v => v.Statut == "EnAttente"),
            virementsEnAttente);

        return View(model);
    }

    public IActionResult Settings() => View();

    public IActionResult About() => View();

    public async Task<IActionResult> Contact()
    {
        var agences = await _context.Agences.AsNoTracking()
            .OrderBy(a => a.Ville).ThenBy(a => a.Nom)
            .ToListAsync();
        return View(agences);
    }
}

public sealed record DepotParDevise(string Devise, decimal Montant);

public sealed record AdminDashboardViewModel(
    int NombreClients,
    int NombreComptes,
    int NombreAgences,
    IReadOnlyList<DepotParDevise> DepotsParDevise,
    int OperationsDuJour,
    int PretsEnCours,
    int AlertesAmlOuvertes,
    int VirementsEnAttente,
    IReadOnlyList<VirementInterAgence> DerniersVirementsEnAttente);
