using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChurchPresenter.Models;
using ChurchPresenter.Services;
using ChurchPresenter.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChurchPresenter.ViewModels;

public partial class BibleViewModel : ViewModelBase
{
    private readonly BibleService _bibleService;
    private readonly PresentationViewModel _presentationViewModel;
    private readonly Action? _clearSongSelection;
    private readonly Action? _clearSongPreview;
    private readonly SemanticSearchService _semanticSearchService;
    private PresentationWindow? _presentationWindow;
    private bool _isInitializing = true;

    [ObservableProperty]
    private ObservableCollection<Bible> bibles = new();

    [ObservableProperty]
    private ObservableCollection<string> books = new();

    [ObservableProperty]
    private Bible? selectedBible;

    [ObservableProperty]
    private string? selectedBook;

    [ObservableProperty]
    private string bookSearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<string, string>> verses = new();

    [ObservableProperty]
    private KeyValuePair<string, string>? selectedVerse;

    partial void OnSelectedVerseChanged(KeyValuePair<string, string>? value)
    {
        // Al seleccionar un versículo, cargarlo automáticamente en ambas columnas (2 y 3)
        // NO cargar en preview durante la inicialización
        if (value.HasValue && !_isInitializing)
        {
            ShowVerse();
        }
    }

    [ObservableProperty]
    private string selectedChapterText = "1";

    [ObservableProperty]
    private string selectedVerseText = "1";

    [ObservableProperty]
    private bool isPresenting;

    [ObservableProperty]
    private string semanticSearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<VerseSearchResult> searchResults = new();

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool isIndexing;

    [ObservableProperty]
    private string indexingProgress = string.Empty;

    public BibleViewModel(BibleService bibleService, PresentationViewModel presentationViewModel, Action? clearProjectedSong = null, Action? clearPreviewSong = null)
    {
        _bibleService = bibleService;
        _presentationViewModel = presentationViewModel;
        _clearSongSelection = clearProjectedSong;
        _clearSongPreview = clearPreviewSong;
        _semanticSearchService = new SemanticSearchService();
        
        // Suscribirse al evento de progreso de indexación
        _semanticSearchService.IndexingProgress += OnIndexingProgress;
        
        _ = LoadBiblesAsync();
    }

    private void OnIndexingProgress(int current, int total)
    {
        var percentage = (current * 100) / total;
        IndexingProgress = $"Indexando: {current}/{total} versículos ({percentage}%)";
    }

    private async Task LoadBiblesAsync()
    {
        var loadedBibles = await _bibleService.GetBiblesAsync();
        Bibles = new ObservableCollection<Bible>(loadedBibles);
        if (Bibles.Any())
        {
            SelectedBible = Bibles[0];
            
            // Cargar Génesis 1:1 por defecto
            BookSearchText = "Génesis";
            SelectedBook = "Génesis";
            SelectedChapterText = "1";
            SelectedVerseText = "1";
            
            // Marcar que la inicialización ha terminado
            _isInitializing = false;
            
            // NOTA: Indexación automática deshabilitada para desarrollo
            // Para indexar manualmente: usar la búsqueda semántica en la pestaña Biblia
            // o el botón 🔍 en la barra de título
            
            /* DESCOMENTAR PARA HABILITAR INDEXACIÓN AUTOMÁTICA EN PRODUCCIÓN
            // Indexar automáticamente la Reina Valera 1960 si está disponible
            var rvr1960 = Bibles.FirstOrDefault(b => 
                b.Name.Contains("Reina Valera 1960", StringComparison.OrdinalIgnoreCase) ||
                b.FileCode?.Contains("RVR1960", StringComparison.OrdinalIgnoreCase) == true);
            
            if (rvr1960 != null)
            {
                Console.WriteLine("Indexando automáticamente Reina Valera 1960 para búsqueda por IA...");
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _semanticSearchService.IndexBibleAsync(rvr1960);
                        Console.WriteLine($"RVR 1960 indexada: {_semanticSearchService.IndexedVersesCount} versículos");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al indexar RVR 1960: {ex.Message}");
                    }
                });
            }
            */
        }
    }

    private void UpdateBooksList()
    {
        if (SelectedBible == null) return;

        var allBooks = new List<string>();
        foreach (var testament in SelectedBible.Testaments.Values)
        {
            allBooks.AddRange(testament.Books.Values.Select(b => b.Name));
        }

        var filteredBooks = string.IsNullOrWhiteSpace(BookSearchText)
            ? allBooks
            : allBooks.Where(b => b.Contains(BookSearchText, StringComparison.OrdinalIgnoreCase));

        Books = new ObservableCollection<string>(filteredBooks);
    }

    partial void OnSelectedBibleChanged(Bible? value)
    {
        if (value != null)
        {
            UpdateBooksList();
            _ = LoadVersesAsync();
        }
    }

    partial void OnSelectedBookChanged(string? value)
    {
        if (value != null)
        {
            _ = LoadVersesAsync();
        }
    }

    partial void OnSelectedChapterTextChanged(string value)
    {
        if (int.TryParse(value, out int _))
        {
            _ = LoadVersesAsync();
        }
    }

    partial void OnSelectedVerseTextChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && Verses.Any())
        {
            if (int.TryParse(value, out int verseNumber))
            {
                var verse = Verses.ElementAtOrDefault(verseNumber - 1);
                if (verse.Key != null)
                {
                    SelectedVerse = verse;
                }
            }
        }
    }

    private async Task LoadVersesAsync()
    {
        if (SelectedBible == null || SelectedBook == null) return;

        var (title, verses) = await _bibleService.GetChapterAsync(
            SelectedBible.Version,
            SelectedBook,
            SelectedChapterText);

        if (verses != null)
        {
            Verses = new ObservableCollection<KeyValuePair<string, string>>(verses);
            if (Verses.Any())
            {
                if (int.TryParse(SelectedVerseText, out int verseNumber))
                {
                    SelectedVerse = Verses.ElementAtOrDefault(verseNumber - 1);
                }
            }
        }
        else
        {
            Verses.Clear();
            SelectedVerse = null;
        }
    }

    [RelayCommand]
    private void ShowVerse()
    {
        if (!SelectedVerse.HasValue) return;

        // Limpiar todas las canciones (columna 2 y columna 3)
        _clearSongSelection?.Invoke();

        // Cargar en AMBAS columnas (2 y 3) para previsualizar y proyectar
        var title = $"{SelectedBook} {SelectedChapterText}:{SelectedVerse.Value.Key}";
        _presentationViewModel.UpdateBiblePresentation(title, SelectedVerse.Value.Value, preview: false);
    }

    [RelayCommand]
    private void ProjectVerse()
    {
        if (!SelectedVerse.HasValue) return;

        var title = $"{SelectedBook} {SelectedChapterText}:{SelectedVerse.Value.Key}";
        
        // Primero actualizar la presentación con el versículo
        _presentationViewModel.UpdateBiblePresentation(title, SelectedVerse.Value.Value, preview: false);
        
        // Luego limpiar la canción proyectada
        _clearSongSelection?.Invoke();

        // Asegurarse de que la ventana de presentación esté abierta
        if (!IsPresenting)
        {
            StartPresentation();
        }
    }

    [RelayCommand]
    private void ShowVerseInPreview()
    {
        if (!SelectedVerse.HasValue) return;

        // Limpiar solo la canción de preview (columna 2), no la proyectada (columna 3)
        _clearSongPreview?.Invoke();

        var title = $"{SelectedBook} {SelectedChapterText}:{SelectedVerse.Value.Key}";
        _presentationViewModel.UpdateBiblePresentation(title, SelectedVerse.Value.Value, preview: true);
    }

    [RelayCommand]
    private void Clear()
    {
        _presentationViewModel.Clear();
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
        if (_presentationWindow != null) return;

        _presentationWindow = new PresentationWindow
        {
            DataContext = _presentationViewModel
        };

        var screen = _presentationWindow.Screens.All;
        if (screen.Count() > 1)
        {
            var presentationScreen = screen.ElementAt(1);
            _presentationWindow.Position = presentationScreen.Bounds.Position;
            _presentationWindow.Width = presentationScreen.Bounds.Width;
            _presentationWindow.Height = presentationScreen.Bounds.Height;
        }

        _presentationWindow.Show();
        IsPresenting = true;
        
        // NO cargar automáticamente el versículo al iniciar la presentación
        // El usuario debe hacer clic en "Proyectar" explícitamente
    }

    private void StopPresentation()
    {
        if (_presentationWindow == null) return;

        _presentationWindow.Close();
        _presentationWindow = null;
        IsPresenting = false;
    }

    public bool FilterBooks(string search, object item)
    {
        if (string.IsNullOrWhiteSpace(search) || item is not string book)
            return true;

        var normalizedSearch = NormalizeText(search);
        var normalizedBook = NormalizeText(book);
        return normalizedBook.Contains(normalizedSearch);
    }

    private string NormalizeText(string text)
    {
        return text.Normalize(System.Text.NormalizationForm.FormD)
                  .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != 
                             System.Globalization.UnicodeCategory.NonSpacingMark)
                  .ToArray()
                  .AsSpan()
                  .ToString()
                  .ToUpperInvariant();
    }

    [RelayCommand]
    private async Task IndexCurrentBibleAsync()
    {
        if (SelectedBible == null)
        {
            Console.WriteLine("No hay ninguna Biblia seleccionada para indexar");
            return;
        }

        IsIndexing = true;
        IndexingProgress = "Iniciando indexación...";
        
        try
        {
            await _semanticSearchService.IndexBibleAsync(SelectedBible);
            IndexingProgress = $"¡Indexación completada! {_semanticSearchService.IndexedVersesCount} versículos listos.";
            Console.WriteLine($"Biblia indexada: {_semanticSearchService.IndexedVersesCount} versículos");
            
            // Limpiar el mensaje después de 3 segundos
            await Task.Delay(3000);
            if (!IsIndexing) // Solo limpiar si ya no está indexando
                IndexingProgress = string.Empty;
        }
        catch (Exception ex)
        {
            IndexingProgress = $"Error: {ex.Message}";
            Console.WriteLine($"Error al indexar la Biblia: {ex.Message}");
        }
        finally
        {
            IsIndexing = false;
        }
    }

    [RelayCommand]
    private async Task SemanticSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SemanticSearchText))
        {
            SearchResults.Clear();
            return;
        }

        if (!_semanticSearchService.IsIndexed)
        {
            Console.WriteLine("La Biblia no ha sido indexada. Indexando...");
            await IndexCurrentBibleAsync();
        }

        IsSearching = true;
        try
        {
            var results = await _semanticSearchService.SearchAsync(SemanticSearchText, 20);
            SearchResults = new ObservableCollection<VerseSearchResult>(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en búsqueda semántica: {ex.Message}");
            SearchResults.Clear();
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectSearchResultAsync(VerseSearchResult result)
    {
        if (result == null) return;

        // Limpiar selección de canciones
        _clearSongSelection?.Invoke();

        // Cargar la cita en la búsqueda de Biblia
        // Esto permite cambiar de versión antes de proyectar
        BookSearchText = result.Book; // Actualizar el texto de búsqueda para el AutoCompleteBox
        SelectedBook = result.Book;
        SelectedChapterText = result.Chapter.ToString();
        SelectedVerseText = result.VerseNumber.ToString();

        // Cargar los versículos del capítulo
        await LoadVersesAsync();
    }
}
