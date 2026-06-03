using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokemonCardScanner.Infrastructure.Data;
using PokemonCardScanner.Infrastructure.Services;
using PokemonCardScanner.Infrastructure.Services.Interfaces;

namespace PokemonCardScanner.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddHttpClient<IPokemonTcgService, PokemonTcgService>();

        services.AddScoped<ICardRecognitionService, CardRecognitionService>();
        services.AddScoped<CardScanOrchestrator>();

        return services;
    }
}
