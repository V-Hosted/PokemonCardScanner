using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokemonCardScanner.Shared.DTOs;
using PokemonCardScanner.Shared.Models;

namespace PokemonCardScanner.MAUI.ViewModels;

[QueryProperty(nameof(ScanResult), "ScanResult")]
public partial class ResultViewModel : ObservableObject
{
    [ObservableProperty]
    private CardScanResponse? _scanResult;

    public PokemonCard? Card => ScanResult?.Card;
    public List<EbaySalePrice> RecentSales => ScanResult?.RecentSales ?? [];
    public bool HasCard => Card is not null;
    public bool HasPrices => RecentSales.Count > 0;
    public string PriceNote => ScanResult?.PricesFromCache == true ? "Prices cached (< 24h old)" : "Live prices";
    public decimal AveragePrice => HasPrices ? RecentSales.Average(p => p.Price) : 0;

    partial void OnScanResultChanged(CardScanResponse? value)
    {
        OnPropertyChanged(nameof(Card));
        OnPropertyChanged(nameof(RecentSales));
        OnPropertyChanged(nameof(HasCard));
        OnPropertyChanged(nameof(HasPrices));
        OnPropertyChanged(nameof(PriceNote));
        OnPropertyChanged(nameof(AveragePrice));
    }

    [RelayCommand]
    private static async Task ScanAnotherAsync()
    {
        await Shell.Current.GoToAsync("//ScanPage");
    }
}
