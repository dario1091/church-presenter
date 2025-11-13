using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ChurchPresenter.Views;

/// <summary>
/// Diálogo para mostrar y gestionar actualizaciones de la aplicación.
/// </summary>
public partial class UpdateDialog : Window
{
    public UpdateDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Maneja el evento de clic en el botón Cerrar.
    /// </summary>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
