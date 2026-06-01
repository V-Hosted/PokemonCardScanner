using Microsoft.AspNetCore.Mvc;
using PokemonCardScanner.Infrastructure.Services;
using PokemonCardScanner.Shared.DTOs;

namespace PokemonCardScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardController(CardScanOrchestrator orchestrator) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<ActionResult<CardScanResponse>> Scan([FromBody] CardScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ImageBase64))
            return BadRequest(new CardScanResponse { Success = false, ErrorMessage = "Image data is required." });

        var result = await orchestrator.ScanAsync(request);
        return result.Success ? Ok(result) : UnprocessableEntity(result);
    }
}
