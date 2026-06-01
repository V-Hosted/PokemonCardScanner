using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services.Interfaces;

public interface IPokemonTcgService
{
    Task<PokemonCard?> FindCardAsync(string name, string? collectorNumber, string? setCode);
}
