using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChurchPresenter.ViewModels;

public partial class PresentationViewModel : ViewModelBase
{
    [ObservableProperty]
    private string currentSongTitle = string.Empty;

    [ObservableProperty]
    private string column1Content = string.Empty;

    [ObservableProperty]
    private string column2Content = string.Empty;

    [ObservableProperty]
    private string column3Content = string.Empty;

    [ObservableProperty]
    private Bitmap? mediaImage;

    [ObservableProperty]
    private string? mediaImagePath;

    [ObservableProperty]
    private Bitmap? projectedMediaImage;

    [ObservableProperty]
    private string? projectedMediaImagePath;

    public void PrepareContent(string title, string content, bool useMultipleColumns = false)
    {
        CurrentSongTitle = title;
        
        // Limpiar imágenes al cargar contenido de texto
        MediaImage = null;
        MediaImagePath = null;
        ProjectedMediaImage = null;
        ProjectedMediaImagePath = null;
        
        if (useMultipleColumns)
        {
            // When using multiple columns, we'll show the content in the middle column only
            Column1Content = string.Empty;
            Column2Content = content;
            Column3Content = string.Empty;
        }
        else
        {
            Column1Content = content;
            Column2Content = string.Empty;
            Column3Content = string.Empty;
        }
    }

    public void UpdatePresentation(string title, string content, bool useMultipleColumns = false)
    {
        PrepareContent(title, content, useMultipleColumns);
    }

    public void UpdateBiblePresentation(string title, string verse, bool preview = true)
    {
        Column1Content = string.Empty;  // Limpiar columna 1
        Column2Content = verse;  // Siempre carga en columna 2 (vista previa)
        
        // Limpiar TODAS las imágenes (preview Y proyección) al cargar contenido de texto
        MediaImage = null;
        MediaImagePath = null;
        ProjectedMediaImage = null;
        ProjectedMediaImagePath = null;
        
        if (!preview)
        {
            // Cuando se proyecta, mostrar cita en título y versículo en contenido
            CurrentSongTitle = title;  // La cita bíblica se muestra arriba
            Column3Content = verse;     // Solo el versículo en el contenido
        }
    }

    public void ProjectCurrentBibleVerse()
    {
        Column3Content = Column2Content;
    }

    public void UpdateMediaPresentation(string title, string imagePath, bool isImage, bool preview = true)
    {
        if (isImage)
        {
            try
            {
                // Cargar en preview (columna 2)
                MediaImage = new Bitmap(imagePath);
                MediaImagePath = imagePath;
                Column1Content = string.Empty; // Limpiar texto de columna 1
                Column2Content = string.Empty; // Limpiar texto de columna 2
                
                // Si es preview, limpiar el título de canción
                if (preview)
                {
                    CurrentSongTitle = string.Empty;
                }
                else
                {
                    // Si no es solo preview, también proyectar (columna 3)
                    CurrentSongTitle = title; // Actualizar título cuando se proyecta
                    ProjectedMediaImage = new Bitmap(imagePath);
                    ProjectedMediaImagePath = imagePath;
                    Column3Content = string.Empty; // Limpiar texto de columna 3
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error cargando imagen: {ex.Message}");
                MediaImage = null;
                MediaImagePath = null;
                if (!preview)
                {
                    ProjectedMediaImage = null;
                    ProjectedMediaImagePath = null;
                }
            }
        }
    }

    public void ProjectCurrentMedia()
    {
        if (MediaImage != null && MediaImagePath != null)
        {
            try
            {
                ProjectedMediaImage = new Bitmap(MediaImagePath);
                ProjectedMediaImagePath = MediaImagePath;
                // Actualizar título con el nombre del archivo al proyectar
                CurrentSongTitle = Path.GetFileName(MediaImagePath);
                Column1Content = string.Empty; // Limpiar texto
                Column3Content = string.Empty; // Limpiar texto
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error proyectando imagen: {ex.Message}");
            }
        }
    }

    public void Clear()
    {
        CurrentSongTitle = string.Empty;
        Column1Content = string.Empty;
        Column2Content = string.Empty;
        Column3Content = string.Empty;
        MediaImage = null;
        MediaImagePath = null;
        ProjectedMediaImage = null;
        ProjectedMediaImagePath = null;
    }
}