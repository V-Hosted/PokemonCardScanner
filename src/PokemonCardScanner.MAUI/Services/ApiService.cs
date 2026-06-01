using System.Net.Http.Json;
using PokemonCardScanner.Shared.DTOs;

namespace PokemonCardScanner.MAUI.Services;

public class ApiService(HttpClient http)
{
    public async Task<CardScanResponse> ScanCardAsync(byte[] imageBytes, string contentType = "image/jpeg")
    {
        var request = new CardScanRequest
        {
            ImageBase64 = Convert.ToBase64String(imageBytes),
            ContentType = contentType
        };

        var response = await http.PostAsJsonAsync("api/card/scan", request);
        var result = await response.Content.ReadFromJsonAsync<CardScanResponse>();
        return result ?? new CardScanResponse { Success = false, ErrorMessage = "Empty response from server." };
    }
}
