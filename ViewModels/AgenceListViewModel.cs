using AkarangaBank.Web.Models; 
namespace AkarangaBank.Web.ViewModels; 
 
// ViewModel utilisé par la page Index des agences. 
// Il contient la liste paginée + les paramètres de recherche. 
public class AgenceListViewModel 
{ 
    public IEnumerable<Agence> Agences{get;set;}=new List<Agence>(); 
    public string? Recherche { get; set; } 
    public int Page { get; set; } = 1; 
    public int TaillePage { get; set; } = 10; 
    public int Total { get; set; } 
 
    // Propriété calculée : nombre total de pages. 
    public int TotalPages =>(int)Math.Ceiling((double)Total / TaillePage); 
}