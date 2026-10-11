using System;
using System.Collections.Generic;
using AkarangaBank.Web.Models;
 
namespace AkarangaBank.Web.ViewModels; 
 
public class CompteListViewModel 
{ 
    public IEnumerable<Compte> Comptes { get; set; } = new List<Compte>(); 
    public string? Recherche { get; set; } 
    public int? AgenceId { get; set; } 
    public int? TypeCompteId { get; set; } 
    public string? Statut { get; set; } 
    public int Page { get; set; } = 1; 
    public int TaillePage { get; set; } = 10; 
    public int Total { get; set; } 
    public int TotalPages => (int)Math.Ceiling((double)Total / TaillePage); 
    public IEnumerable<Agence> Agences { get; set; } = new List<Agence>(); 
    public IEnumerable<TypeCompte> TypesCompte { get; set; } = new List<TypeCompte>(); 
} 