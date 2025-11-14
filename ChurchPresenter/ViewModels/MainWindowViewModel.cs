using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Reflection;

namespace ChurchPresenter.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public SongsViewModel Songs { get; } = new();

    [ObservableProperty]
    private bool isDarkTheme = true;

    /// <summary>
    /// Versión actual de la aplicación (leída dinámicamente del ensamblado).
    /// </summary>
    public string CurrentVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }
    }

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
