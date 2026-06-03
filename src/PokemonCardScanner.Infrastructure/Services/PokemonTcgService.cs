using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PokemonCardScanner.Infrastructure.Services.Interfaces;
using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services;

public class PokemonTcgService(HttpClient http, IConfiguration config) : IPokemonTcgService
{
    public async Task<(PokemonCard? Card, List<CardPrice> Prices)> FindCardAsync(
        string name, string? collectorNumber, string? setCode)
    {
        var apiKey = config["PokemonTcg:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            http.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", apiKey);

        var query = $"name:\"{name}\"";
        if (!string.IsNullOrEmpty(collectorNumber))
            query += $" number:{collectorNumber}";
        if (!string.IsNullOrEmpty(setCode))
            query += $" set.id:{setCode}";

        var url = $"https://api.pokemontcg.io/v2/cards?q={Uri.EscapeDataString(query)}&pageSize=1";

        var response = await http.GetFromJsonAsync<TcgApiResponse>(url);
        var card = response?.Data?.FirstOrDefault();
        if (card is null) return (null, []);

        var pokemonCard = new PokemonCard
        {
            Id = card.Id,
            Name = card.Name,
            SetName = card.Set?.Name ?? string.Empty,
            SetCode = card.Set?.Id ?? string.Empty,
            CollectorNumber = card.Number ?? string.Empty,
            Rarity = card.Rarity ?? string.Empty,
            ImageUrl = card.Images?.Large ?? card.Images?.Small ?? string.Empty,
            Artist = card.Artist
        };

        var prices = ExtractPrices(card.TcgPlayer);
        return (pokemonCard, prices);
    }

    private static List<CardPrice> ExtractPrices(TcgPlayerData? tcgPlayer)
    {
        if (tcgPlayer?.Prices is null) return [];

        var updatedAt = tcgPlayer.UpdatedAt.HasValue ? tcgPlayer.UpdatedAt.Value : (DateTime?)null;
        var result = new List<CardPrice>();

        void AddPrice(string variant, TcgPriceEntry? entry)
        {
            if (entry is null) return;
            result.Add(new CardPrice
            {
                Variant = variant,
                Market = entry.Market,
                Low = entry.Low,
                Mid = entry.Mid,
                High = entry.High,
                Source = "TCGPlayer",
                UpdatedAt = updatedAt
            });
        }

        AddPrice("Normal", tcgPlayer.Prices.Normal);
        AddPrice("Holofoil", tcgPlayer.Prices.Holofoil);
        AddPrice("Reverse Holofoil", tcgPlayer.Prices.ReverseHolofoil);
        AddPrice("1st Edition Holofoil", tcgPlayer.Prices.FirstEditionHolofoil);
        AddPrice("1st Edition Normal", tcgPlayer.Prices.FirstEditionNormal);

        return result;
    }

    // ── JSON models ──────────────────────────────────────────────────────────

    private sealed class TcgApiResponse
    {
        [JsonPropertyName("data")] public List<TcgCard>? Data { get; set; }
    }

    private sealed class TcgCard
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("number")] public string? Number { get; set; }
        [JsonPropertyName("rarity")] public string? Rarity { get; set; }
        [JsonPropertyName("artist")] public string? Artist { get; set; }
        [JsonPropertyName("set")] public TcgSet? Set { get; set; }
        [JsonPropertyName("images")] public TcgImages? Images { get; set; }
        [JsonPropertyName("tcgplayer")] public TcgPlayerData? TcgPlayer { get; set; }
    }

    private sealed class TcgSet
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    }

    private sealed class TcgImages
    {
        [JsonPropertyName("small")] public string? Small { get; set; }
        [JsonPropertyName("large")] public string? Large { get; set; }
    }

    private sealed class TcgPlayerData
    {
        [JsonPropertyName("updatedAt")] public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("prices")] public TcgPrices? Prices { get; set; }
    }

    private sealed class TcgPrices
    {
        [JsonPropertyName("normal")] public TcgPriceEntry? Normal { get; set; }
        [JsonPropertyName("holofoil")] public TcgPriceEntry? Holofoil { get; set; }
        [JsonPropertyName("reverseHolofoil")] public TcgPriceEntry? ReverseHolofoil { get; set; }
        [JsonPropertyName("1stEditionHolofoil")] public TcgPriceEntry? FirstEditionHolofoil { get; set; }
        [JsonPropertyName("1stEditionNormal")] public TcgPriceEntry? FirstEditionNormal { get; set; }
    }

    private sealed class TcgPriceEntry
    {
        [JsonPropertyName("low")] public decimal? Low { get; set; }
        [JsonPropertyName("mid")] public decimal? Mid { get; set; }
        [JsonPropertyName("high")] public decimal? High { get; set; }
        [JsonPropertyName("market")] public decimal? Market { get; set; }
    }
}
