using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PokemonCardScanner.MAUI.Pages;
using PokemonCardScanner.MAUI.Services;
using PokemonCardScanner.MAUI.ViewModels;

namespace PokemonCardScanner.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // API base URL — update to your Azure App Service URL before deploying
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://YOUR_API_APP_SERVICE.azurewebsites.net/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<ResultViewModel>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<ResultPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
