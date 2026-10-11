using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace AkarangaBank.Web.Services;

public class DeclarationService : IDeclarationService
{
    private readonly AkarangaBankContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeclarationService(AkarangaBankContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AlerteAML> CreerAlerteAsync(int clientId, int? operationId, string typeAlerte, string niveau, string description)
    {
        var alerte = new AlerteAML
        {
            ClientId = clientId,
            OperationId = operationId,
            TypeAlerte = typeAlerte,
            Niveau = niveau,
            Description = description,
            Statut = "Ouverte",
            DateAlerte = DateTime.UtcNow
        };
        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.AlertesAML.Add(alerte);
        await _context.SaveChangesAsync();

        var httpContext = _httpContextAccessor.HttpContext;
        var userIdValue = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = int.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;
        var audit = new AuditLog
        {
            DateEvenement = DateTime.UtcNow,
            UtilisateurId = userId,
            Action = "Creation",
            TableConcernee = "AlertesAML",
            EnregistrementId = alerte.Id,
            ValeurApres = JsonSerializer.Serialize(new
            {
                alerte.Id,
                alerte.ClientId,
                alerte.OperationId,
                alerte.TypeAlerte,
                alerte.Niveau,
                alerte.Statut,
                alerte.DateAlerte
            }),
            AdresseIP = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Limiter(httpContext?.Request.Headers.UserAgent.ToString(), 255),
            Details = $"Création de l'alerte AML {alerte.TypeAlerte} ({alerte.Niveau})."
        };
        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return alerte;
    }

    private static string? Limiter(string? value, int maximumLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maximumLength ? value : value[..maximumLength];

    public async Task<bool> TraiterAlerteAsync(int alerteId, int traiteParId, string statut, string? commentaire)
    {
        var alerte = await _context.AlertesAML.FindAsync(alerteId);
        if (alerte is null)
            return false;

        alerte.Statut = statut;
        alerte.TraiteParId = traiteParId;
        alerte.DateTraitement = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(commentaire))
            alerte.Description = $"{alerte.Description} | {commentaire.Trim()}";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AlerteAML>> GetAlertesAsync(string? statut, string? niveau)
    {
        var query = _context.AlertesAML.AsNoTracking()
            .Include(a => a.Client)
            .Include(a => a.Operation)
            .Include(a => a.TraitePar)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(statut))
            query = query.Where(a => a.Statut == statut);
        if (!string.IsNullOrWhiteSpace(niveau))
            query = query.Where(a => a.Niveau == niveau);
        return await query.OrderByDescending(a => a.DateAlerte).ToListAsync();
    }
}
