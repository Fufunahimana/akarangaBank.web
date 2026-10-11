using System.Text;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AkarangaBank.Web.Services;

public class CNRFService : ICNRFService
{
    private readonly AkarangaBankContext _context;

    public CNRFService(AkarangaBankContext context) => _context = context;

    public async Task<DeclarationSoupcon> GenererDOSAsync(int alerteId, int redigeParId)
    {
        var declarationExistante = await _context.DeclarationsSoupcon
            .Include(d => d.Alerte)
            .FirstOrDefaultAsync(d => d.AlerteId == alerteId && d.TypeDeclaration == "DOS");
        if (declarationExistante is not null)
            return declarationExistante;

        var alerte = await _context.AlertesAML
            .Include(a => a.Client)
            .Include(a => a.Operation)
            .FirstOrDefaultAsync(a => a.Id == alerteId)
            ?? throw new InvalidOperationException("Alerte introuvable.");

        var reference = GenererReference("DOS");
        var declaration = new DeclarationSoupcon
        {
            Reference = reference,
            AlerteId = alerte.Id,
            ClientId = alerte.ClientId,
            TypeDeclaration = "DOS",
            Contenu = await GenererContenuDOSAsync(alerte, reference),
            MotifSuspicion = alerte.Description,
            Statut = "Brouillon",
            DateCreation = DateTime.UtcNow,
            RedigeParId = redigeParId
        };

        _context.DeclarationsSoupcon.Add(declaration);
        _context.AuditLogs.Add(new AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = redigeParId,
            Action = "CreationDOS",
            TableConcernee = "DeclarationsSoupcon",
            ValeurApres = reference,
            Details = $"DOS générée à partir de l'alerte AML {alerte.Id}."
        });
        await _context.SaveChangesAsync();
        return declaration;
    }

    // Transmission is a local workflow simulation until CNRF connection details are configured.
    public async Task<bool> TransmettreAsync(int declarationId)
    {
        var declaration = await _context.DeclarationsSoupcon.FirstOrDefaultAsync(d => d.Id == declarationId);
        if (declaration is null || declaration.Statut is not ("Brouillon" or "Validee"))
            return false;

        declaration.Statut = "Transmise";
        declaration.DateTransmission = DateTime.UtcNow;
        _context.AuditLogs.Add(new AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            Action = "TransmissionDOS",
            TableConcernee = "DeclarationsSoupcon",
            EnregistrementId = declaration.Id,
            ValeurApres = "Transmise",
            Details = "Transmission simulée : aucun connecteur CNRF n'est configuré."
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnregistrerAccuseAsync(int declarationId, string reponse)
    {
        if (string.IsNullOrWhiteSpace(reponse) || reponse.Length > 1000)
            throw new ArgumentException("La réponse CNRF doit contenir au plus 1 000 caractères.", nameof(reponse));

        var declaration = await _context.DeclarationsSoupcon.FirstOrDefaultAsync(d => d.Id == declarationId);
        if (declaration is null || declaration.Statut != "Transmise")
            return false;

        declaration.Statut = "Accusee";
        declaration.DateAccuseReception = DateTime.UtcNow;
        declaration.ReponseCNRF = reponse.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DeclarationSoupcon>> GetDeclarationsAsync(string? statut)
    {
        var query = _context.DeclarationsSoupcon
            .Include(d => d.Client)
            .Include(d => d.Alerte)
            .Include(d => d.RedigePar)
            .AsNoTracking()
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(d => d.Statut == statut);
        return await query.OrderByDescending(d => d.DateCreation).ToListAsync();
    }

    public Task<string> GenererContenuDOSAsync(AlerteAML alerte, string? reference = null)
    {
        var contenu = new StringBuilder()
            .AppendLine("DÉCLARATION D'OPÉRATION SUSPECTE (DOS)")
            .AppendLine("Akaranga Bank — Document confidentiel de conformité")
            .AppendLine($"Référence : {reference ?? GenererReference("DOS")}")
            .AppendLine($"Date de génération : {BankTime.ToLocal(DateTime.UtcNow):dd/MM/yyyy HH:mm} (Africa/Bujumbura)")
            .AppendLine()
            .AppendLine("IDENTIFICATION DU CLIENT")
            .AppendLine($"Nom : {alerte.Client.Nom} {alerte.Client.Prenom}")
            .AppendLine($"Numéro client : {alerte.Client.NumeroClient}")
            .AppendLine($"CNI : {alerte.Client.NumeroCNI}")
            .AppendLine($"Téléphone : {alerte.Client.Telephone ?? "Non renseigné"}")
            .AppendLine()
            .AppendLine("OPÉRATION CONCERNÉE");

        if (alerte.Operation is { } operation)
        {
            contenu.AppendLine($"Référence : {operation.Reference}")
                .AppendLine($"Type : {operation.TypeOperation}")
                .AppendLine($"Montant : {operation.Montant:N2} {operation.Devise}")
                .AppendLine($"Date : {BankTime.ToLocal(operation.DateOperation):dd/MM/yyyy HH:mm} (Africa/Bujumbura)");
        }
        else
        {
            contenu.AppendLine("Aucune opération directement associée à cette alerte.");
        }

        contenu.AppendLine()
            .AppendLine("MOTIF DE LA DÉCLARATION")
            .AppendLine($"Type d'alerte : {alerte.TypeAlerte}")
            .AppendLine($"Niveau : {alerte.Niveau}")
            .AppendLine($"Description : {alerte.Description}")
            .AppendLine()
            .AppendLine("Destinataire : Cellule Nationale de Renseignement Financier (CNRF), Bujumbura, Burundi.")
            .AppendLine("Document préparé pour revue et transmission par un responsable habilité.");

        return Task.FromResult(contenu.ToString());
    }

    private static string GenererReference(string prefix) =>
        $"{prefix}-{BankTime.Today:yyyyMMdd}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();
}
