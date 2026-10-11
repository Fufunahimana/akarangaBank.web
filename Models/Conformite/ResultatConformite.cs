namespace AkarangaBank.Web.Models;

public class ResultatConformite
{
    public int CompteId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DateAnalyse { get; set; }
    public bool EstBloquee { get; set; }
    public bool NecessiteDeclaration { get; set; }
    public bool NecessiteVigilanceRenforcee { get; set; }
    public string? Motif { get; set; }
    public NiveauAlerte Niveau { get; set; } = NiveauAlerte.Faible;
}
