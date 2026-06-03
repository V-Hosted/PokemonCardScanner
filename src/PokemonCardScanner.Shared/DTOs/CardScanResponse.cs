using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Shared.DTOs;

public class CardScanResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PokemonCard? Card { get; set; }
    public List<CardPrice> Prices { get; set; } = [];
    public bool PricesFromCache { get; set; }
}
