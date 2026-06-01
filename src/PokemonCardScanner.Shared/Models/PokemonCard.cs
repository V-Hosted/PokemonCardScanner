namespace PokemonCardScanner.Shared.Models;

public class PokemonCard
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SetName { get; set; } = string.Empty;
    public string SetCode { get; set; } = string.Empty;
    public string CollectorNumber { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Artist { get; set; }
}
