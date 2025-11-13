using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChurchPresenter.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public SongsViewModel Songs { get; } = new();

    [ObservableProperty]
    private bool isDarkTheme = true;

    /// <summary>
    /// Comando para cambiar entre tema claro y oscuro.
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = IsDarkTheme 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
    }
}
