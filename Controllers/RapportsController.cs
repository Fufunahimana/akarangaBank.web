using AkarangaBank.Web.Data;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Controllers;

[Authorize(Policy = "AuditeurOnly")]
public class RapportsController : Controller
{
    private readonly AkarangaBankContext _context;

    public RapportsController(AkarangaBankContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> AuditPdf()
    {
        var agences = await _context.Agences.AsNoTracking()
            .OrderBy(a => a.Code)
            .ToListAsync();

        var clients = await _context.Clients.AsNoTracking()
            .Include(c => c.Agence)
            .Include(c => c.Comptes)
                .ThenInclude(c => c.TypeCompte)
            .OrderBy(c => c.Nom)
            .ThenBy(c => c.Prenom)
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
            var type = agence.EstSiege ? "Siege" : "Agence";
            lignes.Add($"{agence.Code} - {agence.Nom} ({agence.Ville}) | {type} | Clients : {nombreClients} | Comptes : {nombreComptes}");
        }

        lignes.Add("");
        lignes.Add("CLIENTS ET LEURS POSSESSIONS");
        foreach (var client in clients)
        {
            lignes.Add($"{client.NumeroClient} - {client.Prenom} {client.Nom} | Agence : {client.Agence?.Code ?? "-"} | Statut : {client.Statut}");
            if (client.Comptes.Count == 0)
            {
                lignes.Add("  Aucun compte bancaire");
                continue;
            }

            foreach (var compte in client.Comptes.OrderBy(c => c.NumeroCompte))
            {
                var type = compte.TypeCompte?.Libelle ?? "Type non renseigne";
                lignes.Add($"  Compte {compte.NumeroCompte} | {type} | Solde : {compte.Solde:N2} {compte.Devise} | Statut : {compte.Statut}");
            }
        }

        var nomFichier = $"rapport-audit-{BankTime.ToLocal(DateTime.UtcNow):yyyyMMdd-HHmmss}.pdf";
        return File(AuditPdfBuilder.Build(lignes), "application/pdf", nomFichier);
    }
}
