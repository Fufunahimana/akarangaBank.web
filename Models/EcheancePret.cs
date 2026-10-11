namespace AkarangaBank.Web.Models; 
 
public class EcheancePret{ 
    public int Id { get; set; } 
    public int PretId { get; set; } 
    public Pret Pret { get; set; } = null!; 
    public int NumeroEcheance { get; set; } 
    public decimal Capital { get; set; } 
    public decimal Interet { get; set; } 
    public decimal MontantTotal { get; set; } 
    public decimal SoldeRestant { get; set; }
    public decimal Penalite { get; set; }
    public DateTime DateEcheance { get; set; } 
    public DateTime? DatePaiement { get; set; } 
    public string Statut { get; set; } = "EnAttente"; 
}
