using PokemonCardScanner.MAUI.ViewModels;

namespace PokemonCardScanner.MAUI.Pages;

public partial class ResultPage : ContentPage
{
    public ResultPage(ResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
