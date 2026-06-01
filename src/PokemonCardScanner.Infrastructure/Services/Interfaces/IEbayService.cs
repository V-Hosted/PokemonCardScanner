using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services.Interfaces;

public interface IEbayService
{
    Task<List<EbaySalePrice>> GetRecentSalesAsync(PokemonCard card, int count = 3);
}
