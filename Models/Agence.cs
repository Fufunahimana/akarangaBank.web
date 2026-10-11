namespace AkarangaBank.Web.Models; 
public class Agence 
{ 
public int Id { get; set; } 
public string Code { get; set; } = string.Empty; 
public string Nom { get; set; } = string.Empty; 
public string Ville { get; set; } = string.Empty; 
public bool EstSiege { get; set; } 
public DateTime DateCreation { get; set; } = DateTime.UtcNow; 
public DateTime? DateModification { get; set; } 
}