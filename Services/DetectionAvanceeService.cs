using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class DetectionAvanceeService : IDetectionAvanceeService
{
    private readonly AkarangaBankContext _context;
    private readonly ISeuilAMLService _seuils;

    public DetectionAvanceeService(AkarangaBankContext context, ISeuilAMLService seuils)
    {
        _context = context;
        _seuils = seuils;
    }

    public async Task<ResultatSurveillance> DetecterStructuringAsync(int clientId, DateTime date)
    {
        var seuil = await _seuils.GetSeuilVigilanceAsync();
        var operations = await _context.Operations.AsNoTracking()
            .Where(o => o.DateOperation >= date.AddDays(-30) && o.DateOperation <= date)
            .Where(o => o.Compte.ClientId == clientId && o.TypeOperation == "Depot")
            .Where(o => o.Statut == "Valide")
            .Where(o => o.Montant >= seuil * 0.8m && o.Montant < seuil)
            .ToListAsync();

        var alertes = new List<AlerteSurveillance>();
        if (operations.Count >= 3)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "Structuring",
                Description = $"{operations.Count} dépôts entre {seuil * 0.8m:N0} et {seuil:N0} BIF sur 30 jours.",
                Niveau = NiveauAlerte.Eleve
            });
        }

        return Resultat(clientId, date, alertes);
    }

    public async Task<ResultatSurveillance> DetecterSmurfingAsync(int clientId, DateTime date)
    {
        var seuil = await _seuils.GetSeuilVigilanceAsync();
        var debut = date.AddDays(-7);
        var comptesAvecDepots = await _context.Operations.AsNoTracking()
            .Where(o => o.DateOperation >= debut && o.DateOperation <= date)
            .Where(o => o.Compte.ClientId == clientId && o.TypeOperation == "Depot")
            .Where(o => o.Statut == "Valide")
            .Where(o => o.Montant > 0 && o.Montant < seuil)
            .GroupBy(o => o.CompteId)
            .Select(group => new { CompteId = group.Key, Total = group.Sum(o => o.Montant) })
            .ToListAsync();

        var total = comptesAvecDepots.Sum(g => g.Total);
        var alertes = new List<AlerteSurveillance>();
        if (comptesAvecDepots.Count >= 2 && total >= seuil)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "Smurfing",
                Description = $"Dépôts sous le seuil totalisant {total:N0} BIF sur {comptesAvecDepots.Count} comptes en 7 jours.",
                Niveau = NiveauAlerte.Critique
            });
        }

        return Resultat(clientId, date, alertes);
    }

    public async Task<ResultatSurveillance> DetecterLayeringAsync(int clientId, DateTime date)
    {
        var nombre = await _context.VirementsInterAgences.AsNoTracking()
            .Where(v => v.DateCreation >= date.AddDays(-14) && v.DateCreation <= date)
            .Where(v => v.Virement.CompteSource.ClientId == clientId && v.Statut != "Rejete")
            .CountAsync();

        var alertes = new List<AlerteSurveillance>();
        if (nombre >= 5)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "Layering",
                Description = $"{nombre} virements inter-agences en 14 jours.",
                Niveau = NiveauAlerte.Eleve
            });
        }

        return Resultat(clientId, date, alertes);
    }

    public async Task<ResultatSurveillance> DetecterVelociteAnormaleAsync(int clientId, DateTime date)
    {
        var nombre = await _context.Operations.AsNoTracking()
            .Where(o => o.DateOperation >= date.AddHours(-1) && o.DateOperation <= date)
            .Where(o => o.Compte.ClientId == clientId)
            .CountAsync();

        var alertes = new List<AlerteSurveillance>();
        if (nombre >= 10)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "VelociteAnormale",
                Description = $"{nombre} opérations en une heure pour le client {clientId}.",
                Niveau = NiveauAlerte.Eleve
            });
        }

        return Resultat(clientId, date, alertes);
    }

    private static ResultatSurveillance Resultat(int clientId, DateTime date, List<AlerteSurveillance> alertes) => new()
    {
        CompteId = 0,
        DateAnalyse = date,
        Alertes = alertes,
        NecessiteDeclaration = alertes.Any(a => a.Niveau >= NiveauAlerte.Eleve)
    };
}
