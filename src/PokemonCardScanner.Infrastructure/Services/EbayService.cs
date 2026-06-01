using Microsoft.Extensions.Configuration;
using PokemonCardScanner.Infrastructure.Services.Interfaces;
using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services;

// TODO: Replace stub with real eBay Browse API calls once credentials are available.
// Required config: Ebay:AppId, Ebay:CertId, Ebay:UserToken
// Docs: https://developer.ebay.com/api-docs/buy/browse/overview.html
public class EbayService(IConfiguration config) : IEbayService
{
    public Task<List<EbaySalePrice>> GetRecentSalesAsync(PokemonCard card, int count = 3)
    {
        // Placeholder — returns empty list until eBay credentials are configured.
        _ = config["Ebay:AppId"]; // will be read once real impl is in place
        return Task.FromResult(new List<EbaySalePrice>());
    }
}
