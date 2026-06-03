using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;
using PokemonCardScanner.Infrastructure.Services.Interfaces;

namespace PokemonCardScanner.Infrastructure.Services;

public class CardRecognitionService(IConfiguration config) : ICardRecognitionService
{
    public async Task<CardOcrResult> RecognizeCardAsync(byte[] imageBytes, string contentType)
    {
        var apiKey = config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic:ApiKey not configured");

        var client = new AnthropicClient(apiKey);

        var base64 = Convert.ToBase64String(imageBytes);
        var mediaType = contentType switch
        {
            "image/png" => "image/png",
            "image/gif" => "image/gif",
            "image/webp" => "image/webp",
            _ => "image/jpeg"
        };

        var messages = new List<Message>
        {
            new()
            {
                Role = RoleType.User,
                Content =
                [
                    new ImageContent
                    {
                        Source = new ImageSource
                        {
                            Type = "base64",
                            MediaType = mediaType,
                            Data = base64
                        }
                    },
                    new TextContent
                    {
                        Text = """
                               This is a Pokemon trading card. Please identify it and respond with ONLY a JSON object in this exact format, no other text:
                               {
                                 "name": "Pokemon name exactly as printed on the card",
                                 "collector_number": "the number before the slash e.g. 025 from 025/198",
                                 "set_code": "the set code if visible e.g. SVI, PAL, OBF",
                                 "confidence": "high/medium/low"
                               }
                               If you cannot identify the card or it is not a Pokemon card, respond with:
                               {"error": "reason"}
                               """
                    }
                ]
            }
        };

        var request = new MessageParameters
        {
            Messages = messages,
            Model = AnthropicModels.Claude3Haiku,
            MaxTokens = 200
        };

        var response = await client.Messages.GetClaudeMessageAsync(request);
        var raw = response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? "";

        return ParseClaudeResponse(raw);
    }

    private static CardOcrResult ParseClaudeResponse(string raw)
    {
        try
        {
            var json = System.Text.Json.JsonDocument.Parse(raw.Trim());
            var root = json.RootElement;

            if (root.TryGetProperty("error", out _))
                return new CardOcrResult(null, null, null, raw);

            var name = root.TryGetProperty("name", out var n) ? n.GetString() : null;
            var number = root.TryGetProperty("collector_number", out var cn) ? cn.GetString() : null;
            var setCode = root.TryGetProperty("set_code", out var sc) ? sc.GetString() : null;

            return new CardOcrResult(name, number, setCode, raw);
        }
        catch
        {
            // Claude returned something unexpected — try to extract a name from raw text
            return new CardOcrResult(null, null, null, raw);
        }
    }
}
