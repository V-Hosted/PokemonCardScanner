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
    AppDbContext db,
    ILogger<CardScanOrchestrator> logger)
{
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

        // Step 1: Identify card via Claude vision
        CardOcrResult ocr;
        try
        {
            ocr = await recognition.RecognizeCardAsync(imageBytes, request.ContentType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Claude vision recognition failed");
            return new CardScanResponse { Success = false, ErrorMessage = "Image recognition failed." };
        }

        if (string.IsNullOrWhiteSpace(ocr.CardName))
        {
            await LogScanAsync(null, false, ocr.RawText);
            return new CardScanResponse { Success = false, ErrorMessage = "Could not identify a Pokemon card in this image." };
        }

        // Step 2: Look up card + prices from Pokemon TCG API
        PokemonCard? card;
        List<CardPrice> prices;
        try
        {
            (card, prices) = await tcg.FindCardAsync(ocr.CardName, ocr.CollectorNumber, ocr.SetCode);
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

        return new CardScanResponse
        {
            Success = true,
            Card = card,
            Prices = prices,
            PricesFromCache = false
        };
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
