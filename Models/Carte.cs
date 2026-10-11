namespace AkarangaBank.Web.Models; 
 
public class Carte 
{ 
    public int Id { get; set; } 
    public string NumeroCarte { get; set; } = string.Empty; 
    public string NumeroCarteMasque { get; set; } = string.Empty;
    public int CompteId { get; set; } 
    public Compte Compte { get; set; } = null!; 
    public int ClientId { get; set; } 
    public Client Client { get; set; } = null!; 
    public string TypeCarte { get; set; } = string.Empty; 
    public DateTime DateExpiration { get; set; } 
    public decimal PlafondRetraitJournalier { get; set; }
    public decimal PlafondPaiementJournalier { get; set; }
    public decimal PlafondPaiementEnLigneJournalier { get; set; }
    public string Statut { get; set; } = "Active"; 
    public string? MotifBlocage { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
    public ICollection<TransactionCarte> Transactions { get; set; } = new List<TransactionCarte>();
}
