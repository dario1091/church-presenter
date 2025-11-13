using Avalonia.Controls;
using Avalonia.Interactivity;
using ChurchPresenter.Models;
using ChurchPresenter.ViewModels;

namespace ChurchPresenter.Views;

public partial class EditSongDialog : Window
{
    public EditSongDialog()
    {
        InitializeComponent();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EditSongViewModel vm)
        {
            vm.Save();
            Close(true);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}