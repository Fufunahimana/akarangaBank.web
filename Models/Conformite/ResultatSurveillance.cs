namespace AkarangaBank.Web.Models;

public class ResultatSurveillance
{
    public int CompteId { get; set; }
    public DateTime DateAnalyse { get; set; }
    public List<AlerteSurveillance> Alertes { get; set; } = new();
    public bool NecessiteDeclaration { get; set; }
}

public class AlerteSurveillance
{
    public string Regle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NiveauAlerte Niveau { get; set; }
}
