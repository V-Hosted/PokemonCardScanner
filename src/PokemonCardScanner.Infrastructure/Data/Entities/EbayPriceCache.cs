namespace PokemonCardScanner.Infrastructure.Data.Entities;

public class EbayPriceCache
{
    public int Id { get; set; }
    public string CardId { get; set; } = string.Empty;
    public string ListingTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime SoldDate { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string ItemUrl { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
}
