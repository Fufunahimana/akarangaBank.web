namespace AkarangaBank.Web.Models; 
 
public class VerificationKYC 
{ 
    public int Id { get; set; }
    public int ClientId{ get; set; } 
    public Client Client { get; set; } = null!; 
    public string TypeDocument { get; set; } = string.Empty; 
    public string NumeroDocument { get; set; } = string.Empty; 
    public string? CheminDocument { get; set; } 
    public string Statut { get; set; } = "EnAttente"; 
    public int? VerifieParId { get; set; } 
    public ApplicationUser? VerifiePar { get; set; } 
    public DateTime? DateVerification { get; set; } 
    public string? Commentaire { get; set; }
    }
