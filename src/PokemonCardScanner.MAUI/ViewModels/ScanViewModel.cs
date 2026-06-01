using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokemonCardScanner.MAUI.Services;
using PokemonCardScanner.Shared.DTOs;

namespace PokemonCardScanner.MAUI.ViewModels;

public partial class ScanViewModel(ApiService apiService) : ObservableObject
{
    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private ImageSource? _capturedImage;

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            StatusMessage = "Camera not supported on this device.";
            return;
        }

        IsScanning = true;
        StatusMessage = "Opening camera...";
        CapturedImage = null;

        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                StatusMessage = "No photo taken.";
                return;
            }

            StatusMessage = "Analysing card...";

            await using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            CapturedImage = ImageSource.FromStream(() => new MemoryStream(bytes));

            var result = await apiService.ScanCardAsync(bytes);
            await NavigateToResult(result);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        IsScanning = true;
        StatusMessage = "Selecting image...";
        CapturedImage = null;

        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is null)
            {
                StatusMessage = "No photo selected.";
                return;
            }

            StatusMessage = "Analysing card...";

            await using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            CapturedImage = ImageSource.FromStream(() => new MemoryStream(bytes));

            var result = await apiService.ScanCardAsync(bytes);
            await NavigateToResult(result);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private static async Task NavigateToResult(CardScanResponse result)
    {
        var parameters = new Dictionary<string, object> { ["ScanResult"] = result };
        await Shell.Current.GoToAsync("//ResultPage", parameters);
    }
}
