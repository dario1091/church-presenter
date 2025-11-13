using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ChurchPresenter.ViewModels;

namespace ChurchPresenter.Views;

public partial class MultimediaView : UserControl
{
    public MultimediaView()
    {
        InitializeComponent();
    }

    private void OnMediaItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && 
            border.Tag is MediaItem mediaItem &&
            DataContext is MultimediaViewModel vm)
        {
            vm.PreviewMediaCommand.Execute(mediaItem);
        }
    }
}
