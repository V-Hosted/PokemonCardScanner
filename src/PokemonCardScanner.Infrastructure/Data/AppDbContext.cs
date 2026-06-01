using Microsoft.EntityFrameworkCore;
using PokemonCardScanner.Infrastructure.Data.Entities;

namespace PokemonCardScanner.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ScanHistory> ScanHistory => Set<ScanHistory>();
    public DbSet<EbayPriceCache> EbayPriceCache => Set<EbayPriceCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScanHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CardId).HasMaxLength(50);
            e.Property(x => x.CardName).HasMaxLength(200);
            e.Property(x => x.SetName).HasMaxLength(200);
            e.Property(x => x.CollectorNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<EbayPriceCache>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CardId).HasMaxLength(50).IsRequired();
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.Property(x => x.Currency).HasMaxLength(10);
            e.HasIndex(x => new { x.CardId, x.CachedAt });
        });
    }
}
