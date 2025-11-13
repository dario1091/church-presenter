using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChurchPresenter.ViewModels;

public partial class MultimediaViewModel : ViewModelBase
{
    private readonly PresentationViewModel _presentationViewModel;
    private readonly string _mediaDirectory;

    [ObservableProperty]
    private ObservableCollection<string> folders = new();

    [ObservableProperty]
    private string? selectedFolder;

    partial void OnSelectedFolderChanged(string? value)
    {
        LoadMediaItems();
    }

    [ObservableProperty]
    private ObservableCollection<MediaItem> mediaItems = new();

    [ObservableProperty]
    private MediaItem? selectedMediaItem;

    public MultimediaViewModel(PresentationViewModel presentationViewModel)
    {
        _presentationViewModel = presentationViewModel;
        // Usar directorio de datos del usuario para media
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChurchPresenter",
            "Media"
        );
        _mediaDirectory = appDataPath;
        
        InitializeDirectories();
        LoadFolders();
        LoadMediaItems();
    }

    private void InitializeDirectories()
    {
        Directory.CreateDirectory(_mediaDirectory);
        
        // Crear carpetas para cada día de culto
        var cultDays = new[] { "Martes", "Jueves", "Sabados", "Domingos-Mañana", "Domingos-Tarde", "General" };
        foreach (var day in cultDays)
        {
            Directory.CreateDirectory(Path.Combine(_mediaDirectory, day));
        }
    }

    private void LoadFolders()
    {
        Folders.Clear();
        Folders.Add("Todos");
        
        if (Directory.Exists(_mediaDirectory))
        {
            var dirs = Directory.GetDirectories(_mediaDirectory)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name);
            
            foreach (var dir in dirs)
            {
                Folders.Add(dir!);
            }
        }
        
        SelectedFolder = "Todos";
    }

    private void LoadMediaItems()
    {
        MediaItems.Clear();
        
        if (!Directory.Exists(_mediaDirectory))
            return;

        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".mkv" };

        // Determinar qué carpetas buscar
        var foldersToSearch = new List<string>();
        
        if (SelectedFolder == "Todos" || string.IsNullOrEmpty(SelectedFolder))
        {
            // Buscar en todas las subcarpetas
            foldersToSearch.AddRange(Directory.GetDirectories(_mediaDirectory));
        }
        else
        {
            // Buscar solo en la carpeta seleccionada
            var folderPath = Path.Combine(_mediaDirectory, SelectedFolder);
            if (Directory.Exists(folderPath))
            {
                foldersToSearch.Add(folderPath);
            }
        }

        foreach (var folder in foldersToSearch)
        {
            var files = Directory.GetFiles(folder);
            var folderName = Path.GetFileName(folder);
            
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToLowerInvariant();
                MediaType type;

                if (imageExtensions.Contains(extension))
                    type = MediaType.Image;
                else if (videoExtensions.Contains(extension))
                    type = MediaType.Video;
                else
                    continue;

                var mediaItem = new MediaItem
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    FolderName = folderName ?? "General",
                    Type = type
                };

                // Cargar thumbnail para imágenes (más pequeño)
                if (type == MediaType.Image)
                {
                    try
                    {
                        mediaItem.Thumbnail = new Bitmap(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error cargando miniatura: {ex.Message}");
                    }
                }

                MediaItems.Add(mediaItem);
            }
        }
    }

    [RelayCommand]
    private async Task AddMediaAsync()
    {
        if (string.IsNullOrEmpty(SelectedFolder) || SelectedFolder == "Todos")
        {
            // Mostrar mensaje para seleccionar una carpeta
            Console.WriteLine("Selecciona una carpeta específica antes de agregar archivos");
            return;
        }

        var options = new FilePickerOpenOptions
        {
            Title = "Seleccionar Multimedia",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Imágenes")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" }
                },
                new FilePickerFileType("Videos")
                {
                    Patterns = new[] { "*.mp4", "*.avi", "*.mov", "*.wmv", "*.mkv" }
                },
                new FilePickerFileType("Todos")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        };

        var files = await App.MainWindow!.StorageProvider.OpenFilePickerAsync(options);

        var targetFolder = Path.Combine(_mediaDirectory, SelectedFolder);
        
        foreach (var file in files)
        {
            try
            {
                var sourcePath = file.Path.LocalPath;
                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(targetFolder, fileName);

                // Copiar archivo si no existe
                if (!File.Exists(destPath))
                {
                    File.Copy(sourcePath, destPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al copiar archivo: {ex.Message}");
            }
        }

        LoadMediaItems();
    }

    [RelayCommand]
    private void DeleteMedia()
    {
        if (SelectedMediaItem == null) return;

        try
        {
            if (File.Exists(SelectedMediaItem.FilePath))
            {
                File.Delete(SelectedMediaItem.FilePath);
            }
            LoadMediaItems();
            SelectedMediaItem = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
        }
    }

    [RelayCommand]
    private void PreviewMedia(MediaItem? item)
    {
        if (item == null) return;

        SelectedMediaItem = item;

        // Cargar preview grande en la columna 2
        if (item.Type == MediaType.Image)
        {
            _presentationViewModel.UpdateMediaPresentation(
                item.FileName,
                item.FilePath,
                isImage: true,
                preview: true
            );
        }
        else
        {
            _presentationViewModel.UpdatePresentation(
                item.FileName,
                "[Vista previa de video]",
                false
            );
        }
    }

    [RelayCommand]
    private void PresentMedia()
    {
        // Proyectar la imagen que está en preview
        _presentationViewModel.ProjectCurrentMedia();
    }
}

public enum MediaType
{
    Image,
    Video
}

public partial class MediaItem : ObservableObject
{
    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string folderName = string.Empty;

    [ObservableProperty]
    private MediaType type;

    [ObservableProperty]
    private Bitmap? thumbnail;
}
