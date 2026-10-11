namespace AkarangaBank.Web.Models; 
 
public class Ecriture 
{ 
    public int Id { get; set; } 
    public int OperationId { get; set; } 
 
    public Operation Operation { get; set; } = null!; 
    public int? CompteId { get; set; }
    public Compte? Compte { get; set; }
    public string? CodeCompteGeneral { get; set; }
    public string Sens { get; set; } = string.Empty;   // "Debit" ou "Credit" 
    public decimal Montant { get; set; } 
    public string Devise { get; set; } = "BIF"; 
    public DateTime DateEcriture { get; set; } = DateTime.UtcNow; 
    public string? Libelle { get; set; } 
}
