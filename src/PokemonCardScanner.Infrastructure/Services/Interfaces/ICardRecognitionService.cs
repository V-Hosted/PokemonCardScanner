namespace PokemonCardScanner.Infrastructure.Services.Interfaces;

public record CardOcrResult(string? CardName, string? CollectorNumber, string? SetCode, string RawText);

public interface ICardRecognitionService
{
    Task<CardOcrResult> RecognizeCardAsync(byte[] imageBytes, string contentType);
}
