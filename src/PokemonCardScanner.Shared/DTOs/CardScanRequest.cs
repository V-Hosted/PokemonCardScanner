namespace PokemonCardScanner.Shared.DTOs;

public class CardScanRequest
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}
