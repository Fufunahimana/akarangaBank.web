namespace AkarangaBank.Web.Models; 
 
public class TypeCompte 
{ 
    public int Id { get; set; } 
    public string Code { get; set; } = string.Empty; 
    public string Libelle { get; set; } = string.Empty; 
    public decimal TauxInteret { get; set; } 
    public decimal SoldeMinimum { get; set; } 
    public bool AutoriseDecouvert { get; set; } 
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
} 