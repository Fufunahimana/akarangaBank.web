using AkarangaBank.Web.Models; 
 
namespace AkarangaBank.Web.ViewModels; 
 
// ViewModel pour la page Détails. 
// Contient l'agence + des compteurs utiles (clients, comptes, utilisateurs). 
public class AgenceDetailsViewModel 
{ 
    public Agence Agence { get; set; } = null!; 

 
    public int NombreClients { get; set; } 
    public int NombreComptes { get; set; } 
    public int NombreUtilisateurs { get; set; }
    }