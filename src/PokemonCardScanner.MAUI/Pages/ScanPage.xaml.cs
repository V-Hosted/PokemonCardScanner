using PokemonCardScanner.MAUI.ViewModels;

namespace PokemonCardScanner.MAUI.Pages;

public partial class ScanPage : ContentPage
{
    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
