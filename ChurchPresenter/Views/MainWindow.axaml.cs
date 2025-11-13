using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ChurchPresenter.Services;
using ChurchPresenter.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;

namespace ChurchPresenter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Abre el diálogo de actualizaciones.
    /// Se llama desde el botón en la barra de título a través del comando.
    /// </summary>
    private async void OnOpenUpdateDialog(object? sender, RoutedEventArgs e)
    {
        // Crear el servicio de actualización con la URL real del repositorio
        var updateService = new UpdateService("https://github.com/dario1091/church-presenter");
        
        // Crear el ViewModel del diálogo
        var updateViewModel = new UpdateViewModel(updateService);
        
        // Crear y mostrar el diálogo
        var dialog = new UpdateDialog
        {
            DataContext = updateViewModel
        };
        
        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// Indexa la Biblia RVR1960 para búsqueda por IA.
    /// </summary>
    private async void OnIndexBibleClick(object? sender, RoutedEventArgs e)
    {
        // Simplemente mostrar un mensaje instruyendo usar la pestaña de Biblia
        var messageWindow = new Window
        {
            Title = "Indexar Biblia",
            Width = 450,
            Height = 200,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Children =
                {
                    new TextBlock 
                    { 
                        Text = "🔍 Indexación de Biblia",
                        FontSize = 18,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    },
                    new TextBlock 
                    { 
                        Text = "Para indexar la Biblia:\n\n" +
                               "1. Ve a la pestaña \"Biblia\"\n" +
                               "2. Selecciona una versión de la Biblia\n" +
                               "3. Haz una búsqueda semántica (ej: \"jesús camina sobre el agua\")\n" +
                               "4. La indexación se hará automáticamente",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        FontSize = 13,
                        TextAlignment = Avalonia.Media.TextAlignment.Left
                    }
                }
            }
        };
        
        await messageWindow.ShowDialog(this);
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}