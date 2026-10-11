namespace AkarangaBank.Web.Models;

public class DeclarationSoupcon
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int AlerteId { get; set; }
    public AlerteAML Alerte { get; set; } = null!;
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string TypeDeclaration { get; set; } = "DOS";
    public string Contenu { get; set; } = string.Empty;
    public string? MotifSuspicion { get; set; }
    public string Statut { get; set; } = "Brouillon";
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateTransmission { get; set; }
    public DateTime? DateAccuseReception { get; set; }
    public int? RedigeParId { get; set; }
    public ApplicationUser? RedigePar { get; set; }
    public string? ReponseCNRF { get; set; }
}
