using AkarangaBank.Web.Models; 
namespace AkarangaBank.Web.ViewModels; 
public class ClientListViewModel{ 
    public IEnumerable<Client> Clients { get; set; } = new 
List<Client>(); 
    public string? Recherche { get; set; } 
    public int? AgenceId { get; set; } 
    public string? Statut { get; set; } 
    public int Page { get; set; } = 1; 
    public int TaillePage { get; set; } = 10; 
    public int Total { get; set; } 
    public int TotalPages => (int)Math.Ceiling((double)Total / TaillePage); 
    // Pour les listes déroulantes de filtre 
    public IEnumerable<Agence> Agences { get; set; } = new List<Agence>();
    }