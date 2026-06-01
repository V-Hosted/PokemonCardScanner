using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PokemonCardScanner.Infrastructure.Services.Interfaces;
using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services;

public class PokemonTcgService(HttpClient http, IConfiguration config) : IPokemonTcgService
{
    public async Task<PokemonCard?> FindCardAsync(string name, string? collectorNumber, string? setCode)
    {
        var apiKey = config["PokemonTcg:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
            http.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", apiKey);

        // Build a query: name is mandatory, narrow by number if available
        var query = $"name:\"{name}\"";
        if (!string.IsNullOrEmpty(collectorNumber))
            query += $" number:{collectorNumber}";

        var url = $"https://api.pokemontcg.io/v2/cards?q={Uri.EscapeDataString(query)}&pageSize=1";

        var response = await http.GetFromJsonAsync<TcgApiResponse>(url);
        var card = response?.Data?.FirstOrDefault();
        if (card is null) return null;

        return new PokemonCard
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
    }

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
}
