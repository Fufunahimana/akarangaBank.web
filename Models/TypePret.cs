namespace AkarangaBank.Web.Models;

public class TypePret
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal MontantMin { get; set; }
    public decimal MontantMax { get; set; }
    public int DureeMinMois { get; set; }
    public int DureeMaxMois { get; set; }
    public decimal TauxMin { get; set; }
    public decimal TauxMax { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
