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
    public List<CardPrice> Prices => ScanResult?.Prices ?? [];
    public bool HasCard => Card is not null;
    public bool HasPrices => Prices.Count > 0;
    public string PriceSource => HasPrices ? $"Prices via TCGPlayer · Updated {Prices[0].UpdatedAt:MMM d, yyyy}" : string.Empty;

    partial void OnScanResultChanged(CardScanResponse? value)
    {
        OnPropertyChanged(nameof(Card));
        OnPropertyChanged(nameof(Prices));
        OnPropertyChanged(nameof(HasCard));
        OnPropertyChanged(nameof(HasPrices));
        OnPropertyChanged(nameof(PriceSource));
    }

    [RelayCommand]
    private static async Task ScanAnotherAsync()
    {
        await Shell.Current.GoToAsync("//ScanPage");
    }
}
