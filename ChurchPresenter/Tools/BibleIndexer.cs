using System;
using System.IO;
using System.Threading.Tasks;
using ChurchPresenter.Services;

namespace ChurchPresenter.Tools;

/// <summary>
/// Herramienta para pre-indexar biblias y generar cachés antes de compilar
/// </summary>
public class BibleIndexer
{
    public static async Task IndexBibleAsync(string bibleName)
    {
        Console.WriteLine($"=== Indexador de Biblias ===");
        Console.WriteLine($"Indexando: {bibleName}");
        
        var bibleService = new BibleService();
        var semanticSearchService = new SemanticSearchService();
        
        // Suscribirse al progreso
        semanticSearchService.IndexingProgress += (current, total) =>
        {
            var percentage = (current * 100) / total;
            Console.WriteLine($"Progreso: {current}/{total} ({percentage}%)");
        };
        
        // Cargar la biblia
        var bibles = await bibleService.GetBiblesAsync();
        var bible = bibles.Find(b => 
            b.Name.Contains(bibleName, StringComparison.OrdinalIgnoreCase) ||
            b.FileCode?.Contains(bibleName, StringComparison.OrdinalIgnoreCase) == true);
        
        if (bible == null)
        {
            Console.WriteLine($"Error: No se encontró la biblia '{bibleName}'");
            Console.WriteLine("Biblias disponibles:");
            foreach (var b in bibles)
            {
                Console.WriteLine($"  - {b.Name} ({b.FileCode})");
            }
            return;
        }
        
        Console.WriteLine($"Biblia encontrada: {bible.Name}");
        Console.WriteLine("Iniciando indexación...");
        
        try
        {
            await semanticSearchService.IndexBibleAsync(bible);
            Console.WriteLine($"✓ Indexación completada: {semanticSearchService.IndexedVersesCount} versículos");
            Console.WriteLine($"El caché ha sido guardado y estará disponible en la aplicación.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error durante la indexación: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        
        semanticSearchService.Dispose();
    }
    
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Uso: dotnet run --project ChurchPresenter/Tools/BibleIndexer.cs -- <nombre_biblia>");
            Console.WriteLine("Ejemplo: dotnet run --project ChurchPresenter/Tools/BibleIndexer.cs -- \"Reina Valera 1960\"");
            return;
        }
        
        var bibleName = string.Join(" ", args);
        await IndexBibleAsync(bibleName);
    }
}
