using System.ComponentModel.DataAnnotations;

namespace AkarangaBank.Web.ViewModels;

public class PretRejetViewModel
{
    public int PretId { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    public string Motif { get; set; } = string.Empty;
}
