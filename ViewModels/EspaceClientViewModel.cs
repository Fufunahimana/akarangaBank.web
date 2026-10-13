using AkarangaBank.Web.Models;

namespace AkarangaBank.Web.ViewModels;

public sealed class EspaceClientViewModel
{
    public string NomComplet { get; init; } = string.Empty;
    public Client? Client { get; init; }
    public IReadOnlyList<Compte> Comptes { get; init; } = Array.Empty<Compte>();
    public IReadOnlyList<Ecriture> ActiviteRecente { get; init; } = Array.Empty<Ecriture>();
    public IReadOnlyList<Agence> Agences { get; init; } = Array.Empty<Agence>();
}
