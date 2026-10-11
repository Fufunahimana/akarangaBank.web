namespace AkarangaBank.Web.Models; 
 
public class Parametre 
{ 
    public int Id { get; set; } 
    public string Cle { get; set; } = string.Empty; 
    public string Valeur { get; set; } = string.Empty; 
    public string? Description { get; set; } 
    public int? AgenceId { get; set; } 
    public Agence? Agence { get; set; } 
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
    }