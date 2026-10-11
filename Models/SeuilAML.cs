namespace AkarangaBank.Web.Models;

public class SeuilAML
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string Devise { get; set; } = "BIF";
    public string TypeOperation { get; set; } = "Toutes";
    public bool Actif { get; set; } = true;
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
}
