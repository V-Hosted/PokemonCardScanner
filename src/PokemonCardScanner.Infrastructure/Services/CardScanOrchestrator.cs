using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokemonCardScanner.Infrastructure.Data;
using PokemonCardScanner.Infrastructure.Data.Entities;
using PokemonCardScanner.Infrastructure.Services.Interfaces;
using PokemonCardScanner.Shared.DTOs;
using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.Infrastructure.Services;

public class CardScanOrchestrator(
    ICardRecognitionService recognition,
    IPokemonTcgService tcg,
    IEbayService ebay,
    AppDbContext db,
    ILogger<CardScanOrchestrator> logger)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<CardScanResponse> ScanAsync(CardScanRequest request)
    {
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.ImageBase64);
        }
        catch (FormatException)
        {
            return new CardScanResponse { Success = false, ErrorMessage = "Invalid image data." };
        }

        // Step 1: OCR the card image
        CardOcrResult ocr;
        try
        {
            ocr = await recognition.RecognizeCardAsync(imageBytes, request.ContentType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Vision OCR failed");
            return new CardScanResponse { Success = false, ErrorMessage = "Image recognition failed." };
        }

        if (string.IsNullOrWhiteSpace(ocr.CardName))
        {
            await LogScanAsync(null, false, ocr.RawText);
            return new CardScanResponse { Success = false, ErrorMessage = "Could not read card name from image." };
        }

        // Step 2: Match against Pokemon TCG API
        PokemonCard? card;
        try
        {
            card = await tcg.FindCardAsync(ocr.CardName, ocr.CollectorNumber, ocr.SetCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pokemon TCG API lookup failed for '{Name}'", ocr.CardName);
            return new CardScanResponse { Success = false, ErrorMessage = "Card lookup failed." };
        }

        if (card is null)
        {
            await LogScanAsync(null, false, ocr.RawText, ocr.CardName);
            return new CardScanResponse { Success = false, ErrorMessage = $"No card found matching '{ocr.CardName}'." };
        }

        await LogScanAsync(card, true, ocr.RawText);

        // Step 3: Get eBay prices (cached)
        var (prices, fromCache) = await GetPricesAsync(card);

        return new CardScanResponse
        {
            Success = true,
            Card = card,
            RecentSales = prices,
            PricesFromCache = fromCache
        };
    }

    private async Task<(List<EbaySalePrice> prices, bool fromCache)> GetPricesAsync(PokemonCard card)
    {
        var cutoff = DateTime.UtcNow - CacheDuration;
        var cached = await db.EbayPriceCache
            .Where(e => e.CardId == card.Id && e.CachedAt >= cutoff)
            .OrderByDescending(e => e.SoldDate)
            .Take(3)
            .ToListAsync();

        if (cached.Count > 0)
        {
            return (cached.Select(c => new EbaySalePrice
            {
                Price = c.Price,
                Currency = c.Currency,
                SoldDate = c.SoldDate,
                Condition = c.Condition,
                ListingTitle = c.ListingTitle,
                ItemUrl = c.ItemUrl
            }).ToList(), true);
        }

        List<EbaySalePrice> fresh;
        try
        {
            fresh = await ebay.GetRecentSalesAsync(card);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "eBay price lookup failed for card '{Id}'", card.Id);
            return ([], false);
        }

        if (fresh.Count > 0)
        {
            var now = DateTime.UtcNow;
            db.EbayPriceCache.AddRange(fresh.Select(p => new EbayPriceCache
            {
                CardId = card.Id,
                ListingTitle = p.ListingTitle,
                Price = p.Price,
                Currency = p.Currency,
                SoldDate = p.SoldDate,
                Condition = p.Condition,
                ItemUrl = p.ItemUrl,
                CachedAt = now
            }));
            await db.SaveChangesAsync();
        }

        return (fresh, false);
    }

    private async Task LogScanAsync(PokemonCard? card, bool found, string rawText, string? attemptedName = null)
    {
        db.ScanHistory.Add(new ScanHistory
        {
            ScannedAt = DateTime.UtcNow,
            CardId = card?.Id,
            CardName = card?.Name ?? attemptedName,
            SetName = card?.SetName,
            CollectorNumber = card?.CollectorNumber,
            RawOcrText = rawText,
            CardFound = found
        });
        await db.SaveChangesAsync();
    }
}
