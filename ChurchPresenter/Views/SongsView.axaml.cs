using Avalonia.Controls;
using Avalonia.Interactivity;
using ChurchPresenter.Models;
using ChurchPresenter.ViewModels;

namespace ChurchPresenter.Views;

public partial class SongsView : UserControl
{
    public SongsView()
    {
        InitializeComponent();
    }

    private void OnVerseDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SongsViewModel vm && 
            sender is ListBox listBox && 
            listBox.SelectedItem is Verse verse)
        {
            vm.PresentVerseCommand.Execute(verse);
        }
    }

    private void OnBibleVersePreviewDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SongsViewModel vm)
        {
            vm.BibleViewModel.ProjectVerseCommand.Execute(null);
        }
    }

    private void OnMediaPreviewDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SongsViewModel vm)
        {
            vm.MultimediaViewModel.PresentMediaCommand.Execute(null);
        }
    }
}