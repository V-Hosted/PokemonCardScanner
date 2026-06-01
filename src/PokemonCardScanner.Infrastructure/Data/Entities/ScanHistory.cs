namespace PokemonCardScanner.Infrastructure.Data.Entities;

public class ScanHistory
{
    public int Id { get; set; }
    public DateTime ScannedAt { get; set; }
    public string? CardId { get; set; }
    public string? CardName { get; set; }
    public string? SetName { get; set; }
    public string? CollectorNumber { get; set; }
    public string? RawOcrText { get; set; }
    public bool CardFound { get; set; }
}
