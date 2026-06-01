using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;
using PokemonCardScanner.Infrastructure.Services.Interfaces;

namespace PokemonCardScanner.Infrastructure.Services;

public class CardRecognitionService(IConfiguration config) : ICardRecognitionService
{
    public async Task<CardOcrResult> RecognizeCardAsync(byte[] imageBytes, string contentType)
    {
        var endpoint = config["AzureVision:Endpoint"]
            ?? throw new InvalidOperationException("AzureVision:Endpoint not configured");
        var key = config["AzureVision:Key"]
            ?? throw new InvalidOperationException("AzureVision:Key not configured");

        var client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));

        using var stream = new MemoryStream(imageBytes);
        var result = await client.AnalyzeAsync(
            BinaryData.FromStream(stream),
            VisualFeatures.Read);

        var allText = string.Join(" ", result.Value.Read.Blocks
            .SelectMany(b => b.Lines)
            .Select(l => l.Text));

        var cardName = ExtractCardName(result.Value.Read);
        var collectorNumber = ExtractCollectorNumber(allText);

        return new CardOcrResult(cardName, collectorNumber, null, allText);
    }

    private static string? ExtractCardName(ReadResult read)
    {
        // The card name is typically the largest text at the top of the card.
        // We take the first non-numeric line with more than 2 characters.
        return read.Blocks
            .SelectMany(b => b.Lines)
            .Select(l => l.Text.Trim())
            .FirstOrDefault(t => t.Length > 2 && !t.All(c => char.IsDigit(c) || c == '/'));
    }

    private static string? ExtractCollectorNumber(string text)
    {
        // Collector numbers appear as "NNN/TTT" e.g. "025/198"
        var match = System.Text.RegularExpressions.Regex.Match(text, @"\b(\d{1,4})/(\d{1,4})\b");
        return match.Success ? match.Groups[1].Value : null;
    }
}
