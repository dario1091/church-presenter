using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ChurchPresenter.Services;

namespace ChurchPresenter.Views;

public partial class BibleView : UserControl
{
    public BibleView()
    {
        InitializeComponent();
        if (this.FindControl<AutoCompleteBox>("bookAutoComplete") is AutoCompleteBox autoComplete)
        {
            autoComplete.TextFilter = (search, item) =>
            {
                if (DataContext is ViewModels.BibleViewModel vm && 
                    search is not null && 
                    item is not null)
                {
                    return vm.FilterBooks(search, item);
                }
                return true;
            };
        }
    }

    private void OnBookSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is AutoCompleteBox autoComplete && 
            DataContext is ViewModels.BibleViewModel vm)
        {
            vm.SelectedBook = autoComplete.SelectedItem as string;
        }
    }

    private void OnSearchResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && 
            border.DataContext is VerseSearchResult result &&
            DataContext is ViewModels.BibleViewModel vm)
        {
            vm.SelectSearchResultCommand.Execute(result);
        }
    }
}
