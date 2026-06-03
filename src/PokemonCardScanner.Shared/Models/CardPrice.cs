namespace PokemonCardScanner.Shared.Models;

public class CardPrice
{
    public string Variant { get; set; } = string.Empty;  // e.g. "Holofoil", "Normal", "Reverse Holofoil"
    public decimal? Market { get; set; }
    public decimal? Low { get; set; }
    public decimal? Mid { get; set; }
    public decimal? High { get; set; }
    public string Source { get; set; } = "TCGPlayer";
    public DateTime? UpdatedAt { get; set; }
}
