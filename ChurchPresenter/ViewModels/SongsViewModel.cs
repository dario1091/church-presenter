using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using ChurchPresenter.Models;
using ChurchPresenter.Services;
using ChurchPresenter.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChurchPresenter.ViewModels;

public partial class SongsViewModel : ViewModelBase
{
    private readonly SongService _songService;
    private ObservableCollection<Song> _allSongs = new();
    private PresentationWindow? _presentationWindow;
    private PresentationViewModel _presentationViewModel = new();
    
    public PresentationViewModel Presentation => _presentationViewModel;
    
    [ObservableProperty]
    private bool isPresenting;

    [ObservableProperty]
    private BibleViewModel bibleViewModel;

    [ObservableProperty]
    private MultimediaViewModel multimediaViewModel;

    [ObservableProperty]
    private ObservableCollection<Song> filteredSongs = new();

    [ObservableProperty]
    private Song? selectedSong;

    partial void OnSelectedSongChanged(Song? value)
    {
        // Al seleccionar una canción, limpiar el preview bíblico (columna 2)
        // pero NO tocar la proyección (columna 3) hasta que se proyecte explícitamente
        if (value != null)
        {
            // Limpiar el contenido bíblico de la columna 2 (preview)
            _presentationViewModel.Column2Content = string.Empty;
            
            // Do NOT clear Column3Content or ProjectedMediaImage here —
            // projection must remain unchanged until the user explicitly projects (double-click).
            // Only clear preview media so the song preview is shown cleanly.
            _presentationViewModel.MediaImage = null;
            _presentationViewModel.MediaImagePath = null;
        }
    }

    [ObservableProperty]
    private Song? presentingSong;

    [ObservableProperty]
    private Verse? selectedVerse;

    [ObservableProperty]
    private Verse? presentingVerse;

    [ObservableProperty]
    private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        FilterSongs();
    }

    public SongsViewModel()
    {
        _songService = new SongService();
        var bibleService = new BibleService();
        _presentationViewModel = new PresentationViewModel();
        BibleViewModel = new BibleViewModel(
            bibleService, 
            _presentationViewModel, 
            clearProjectedSong: () => 
            { 
                // Limpiar todas las referencias a canciones (columna 2 y 3)
                SelectedSong = null; 
                SelectedVerse = null;
                PresentingSong = null;
                PresentingVerse = null;
            },
            clearPreviewSong: () =>
            {
                // Limpiar solo la canción de preview (columna 2)
                SelectedSong = null;
                SelectedVerse = null;
            });
        MultimediaViewModel = new MultimediaViewModel(_presentationViewModel);
        LoadSongs();
    }

    private async void LoadSongs()
    {
        var songs = await _songService.LoadAllSongsAsync();
        _allSongs = new ObservableCollection<Song>(songs);
        FilterSongs();

        // Add demo song if no songs exist
        if (_allSongs.Count == 0)
        {
            var demoSong = new Song
            {
                Title = "Amazing Grace",
                Author = "John Newton",
                Verses = new List<Verse>
                {
                    new() { Type = VerseType.Verse, Order = 1, Label = "Verse 1", 
                           Content = "Amazing grace how sweet the sound\nThat saved a wretch like me\nI once was lost, but now I'm found\nWas blind, but now I see" },
                    new() { Type = VerseType.Chorus, Order = 2, Label = "Chorus",
                           Content = "My chains are gone, I've been set free\nMy God, my Savior has ransomed me" },
                    new() { Type = VerseType.Verse, Order = 3, Label = "Verse 2",
                           Content = "Through many dangers, toils and snares\nI have already come\n'Tis grace has brought me safe thus far\nAnd grace will lead me home" }
                }
            };
            await _songService.SaveSongAsync(demoSong);
            _allSongs.Add(demoSong);
            FilterSongs();
        }
    }

    private void FilterSongs()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allSongs
            : _allSongs.Where(s => s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                 s.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        FilteredSongs = new ObservableCollection<Song>(filtered);
    }

    [RelayCommand]
    private void PresentVerse(Verse verse)
    {
        if (SelectedSong != null)
        {
            PresentingSong = SelectedSong;
            PresentingVerse = verse;
            
            // Limpiar el contenido de la Biblia/multimedia cuando se proyecta una canción
            _presentationViewModel.CurrentSongTitle = SelectedSong.Title;
            _presentationViewModel.Column2Content = string.Empty;
            _presentationViewModel.Column3Content = string.Empty;
            _presentationViewModel.Column1Content = string.Empty;
            _presentationViewModel.MediaImage = null;
            _presentationViewModel.MediaImagePath = null;
            _presentationViewModel.ProjectedMediaImage = null;
            _presentationViewModel.ProjectedMediaImagePath = null;
        }
    }

    [RelayCommand]
    private void TogglePresentation()
    {
        if (IsPresenting)
        {
            StopPresentation();
        }
        else
        {
            StartPresentation();
        }
    }

    private void StartPresentation()
    {
        if (_presentationWindow == null)
        {
            _presentationWindow = new PresentationWindow
            {
                DataContext = this
            };

            var screen = _presentationWindow.Screens.All;
            if (screen.Count() > 1)
            {
                // Usar la segunda pantalla si está disponible
                var presentationScreen = screen.ElementAt(1);
                _presentationWindow.Position = presentationScreen.Bounds.Position;
                _presentationWindow.Width = presentationScreen.Bounds.Width;
                _presentationWindow.Height = presentationScreen.Bounds.Height;
            }

            _presentationWindow.Show();
            IsPresenting = true;

            // No necesitamos llamar a UpdatePresentation para canciones
            // porque usan PresentingSong y PresentingVerse directamente
        }
    }

    private void StopPresentation()
    {
        if (_presentationWindow != null)
        {
            _presentationWindow.Close();
            _presentationWindow = null;
            IsPresenting = false;
        }
    }

    [RelayCommand]
    private async Task AddSongAsync()
    {
        var dialog = new EditSongDialog
        {
            DataContext = new EditSongViewModel()
        };

        var result = await dialog.ShowDialog<bool>(App.MainWindow);
        if (result)
        {
            var newSong = ((EditSongViewModel)dialog.DataContext).Save();
            await _songService.SaveSongAsync(newSong);
            _allSongs.Add(newSong);
            FilterSongs();
        }
    }

    [RelayCommand]
    private async Task EditSongAsync()
    {
        if (SelectedSong == null) return;

        var dialog = new EditSongDialog
        {
            DataContext = new EditSongViewModel(SelectedSong)
        };

        var result = await dialog.ShowDialog<bool>(App.MainWindow);
        if (result)
        {
            var editedSong = ((EditSongViewModel)dialog.DataContext).Save();
            await _songService.SaveSongAsync(editedSong);
            FilterSongs();
        }
    }

    [RelayCommand]
    private async Task DeleteSongAsync()
    {
        if (SelectedSong == null) return;

        var result = await ShowConfirmationDialog(
            "Confirmar Eliminación",
            $"¿Está seguro que desea eliminar la canción \"{SelectedSong.Title}\"?",
            "Sí",
            "No"
        );

        if (result)
        {
            if (File.Exists(SelectedSong.FilePath))
            {
                File.Delete(SelectedSong.FilePath);
            }
            _allSongs.Remove(SelectedSong);
            FilterSongs();
            SelectedSong = null;
        }
    }

    private async Task<bool> ShowConfirmationDialog(string title, string message, string confirmText, string cancelText)
    {
        var dialog = new Window
        {
            Width = 480,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SystemDecorations = SystemDecorations.None,
            TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur },
            Background = Brushes.Transparent
        };

        // Card principal con borde y sombra
        var card = new Border
        {
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Blur = 30,
                Color = Color.Parse("#60000000"),
                OffsetX = 0,
                OffsetY = 8
            })
        };

        // Binding dinámico de colores según el tema
        card.Bind(Border.BackgroundProperty, 
            Application.Current!.GetResourceObservable("CardBackground"));
        card.Bind(Border.BorderBrushProperty,
            Application.Current!.GetResourceObservable("CardBorderBrush"));
        card.BorderThickness = new Thickness(1);

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(0),
            Spacing = 0
        };

        // Header con título
        var header = new Border
        {
            Padding = new Thickness(24, 18),
            CornerRadius = new CornerRadius(12, 12, 0, 0)
        };

        header.Bind(Border.BackgroundProperty,
            Application.Current!.GetResourceObservable("CardHeaderBackground"));

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold
        };

        titleText.Bind(TextBlock.ForegroundProperty,
            Application.Current!.GetResourceObservable("TextControlForeground"));

        header.Child = titleText;

        // Contenido del mensaje
        var contentBorder = new Border
        {
            Padding = new Thickness(24, 20, 24, 20)
        };

        var contentPanel = new StackPanel
        {
            Spacing = 24
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            LineHeight = 22
        };

        messageText.Bind(TextBlock.ForegroundProperty,
            Application.Current!.GetResourceObservable("TextControlForeground"));

        // Panel de botones
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 12
        };

        var cancelButton = new Button
        {
            Content = cancelText,
            MinWidth = 100,
            Padding = new Thickness(20, 10)
        };

        var confirmButton = new Button
        {
            Content = confirmText,
            MinWidth = 100,
            Padding = new Thickness(20, 10),
            Classes = { "accent" }
        };

        bool? result = null;
        confirmButton.Click += (s, e) => { result = true; dialog.Close(); };
        cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(confirmButton);
        
        contentPanel.Children.Add(messageText);
        contentPanel.Children.Add(buttonPanel);
        
        contentBorder.Child = contentPanel;

        mainPanel.Children.Add(header);
        mainPanel.Children.Add(contentBorder);
        
        card.Child = mainPanel;
        dialog.Content = card;

        await dialog.ShowDialog(App.MainWindow);

        return result == true;
    }

    [RelayCommand]
    private async Task ExportSongsAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "Exportar Canciones",
            DefaultExtension = "json",
            SuggestedFileName = "canciones_exportadas.json",
            FileTypeChoices = new[] 
            { 
                new Avalonia.Platform.Storage.FilePickerFileType("JSON")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        };

        var file = await App.MainWindow!.StorageProvider.SaveFilePickerAsync(dialog);
        
        if (file != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(_allSongs, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(file.Path.LocalPath, json);
                
                // Mostrar mensaje de éxito
                await ShowMessageAsync("Exportación Exitosa", $"Se exportaron {_allSongs.Count} canciones correctamente.");
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Error al exportar canciones: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task ImportSongsAsync()
    {
        var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Importar Canciones",
            AllowMultiple = false,
            FileTypeFilter = new[] 
            { 
                new Avalonia.Platform.Storage.FilePickerFileType("JSON")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        };

        var files = await App.MainWindow!.StorageProvider.OpenFilePickerAsync(dialog);
        
        if (files.Count > 0)
        {
            try
            {
                var json = await File.ReadAllTextAsync(files[0].Path.LocalPath);
                var importedSongs = JsonSerializer.Deserialize<List<Song>>(json);
                
                if (importedSongs != null)
                {
                    int imported = 0;
                    foreach (var song in importedSongs)
                    {
                        // Verificar si la canción ya existe
                        if (!_allSongs.Any(s => s.Title == song.Title && s.Author == song.Author))
                        {
                            song.FilePath = string.Empty; // Se generará al guardar
                            await _songService.SaveSongAsync(song);
                            _allSongs.Add(song);
                            imported++;
                        }
                    }
                    
                    FilterSongs();
                    await ShowMessageAsync("Importación Exitosa", $"Se importaron {imported} canciones correctamente.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Error", $"Error al importar canciones: {ex.Message}");
            }
        }
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var messageBox = new Window
        {
            Title = title,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });
        
        var okButton = new Button 
        { 
            Content = "OK", 
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0),
            Width = 80
        };
        
        okButton.Click += (s, e) => messageBox.Close();
        
        panel.Children.Add(okButton);
        messageBox.Content = panel;

        await messageBox.ShowDialog(App.MainWindow);
    }
}