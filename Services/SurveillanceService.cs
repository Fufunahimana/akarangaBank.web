using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class SurveillanceService : ISurveillanceService
{
    private readonly AkarangaBankContext _context;
    private readonly IConformiteService _conformite;

    public SurveillanceService(AkarangaBankContext context, IConformiteService conformite)
    {
        _context = context;
        _conformite = conformite;
    }

    public async Task<ResultatSurveillance> AnalyserTransactionAsync(int compteId, decimal montant, string typeOperation, DateTime date)
    {
        var alertes = new List<AlerteSurveillance>();
        var compte = await _context.Comptes.FirstOrDefaultAsync(c => c.Id == compteId);
        if (compte is null)
            return new ResultatSurveillance { CompteId = compteId, DateAnalyse = date };

        var seuil = await _conformite.GetSeuilAMLAsync();
        if (montant >= seuil)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "MontantEleve",
                Description = $"Transaction de {montant:N0} BIF dépassant le seuil AML.",
                Niveau = NiveauAlerte.Moyen
            });
        }

        var debut = date.AddDays(-7);
        var nombre = await _context.Operations.CountAsync(o => o.CompteId == compteId && o.DateOperation >= debut && o.DateOperation <= date);
        if (nombre > 20)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "FrequenceElevee",
                Description = $"{nombre} transactions sur ce compte en 7 jours.",
                Niveau = NiveauAlerte.Eleve
            });
        }

        if (await _conformite.EvaluerRisqueClientAsync(compte.ClientId) == NiveauRisqueClient.Eleve)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "ClientARisque",
                Description = "Le client associé est classé à risque élevé.",
                Niveau = NiveauAlerte.Eleve
            });
        }

        var moyenne = await _context.Operations.Where(o => o.CompteId == compteId)
            .Select(o => (decimal?)o.Montant)
            .AverageAsync() ?? 0m;
        if (montant >= seuil && moyenne > 0 && montant > moyenne * 5)
        {
            alertes.Add(new AlerteSurveillance
            {
                Regle = "MontantInhabituel",
                Description = $"Montant {montant:N0} BIF supérieur à 5 fois la moyenne ({moyenne:N0} BIF).",
                Niveau = NiveauAlerte.Moyen
            });
        }

        return new ResultatSurveillance
        {
            CompteId = compteId,
            DateAnalyse = date,
            Alertes = alertes,
            NecessiteDeclaration = alertes.Any(a => a.Niveau >= NiveauAlerte.Eleve)
        };
    }

    public Task<NiveauRisqueClient> EvaluerRisqueClientAsync(int clientId) => _conformite.EvaluerRisqueClientAsync(clientId);
}
