using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChurchPresenter.Services;

public class CachedEmbedding
{
    public string Key { get; set; } = string.Empty; // BibleVersion_Book_Chapter_Verse
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

public class EmbeddingCache
{
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CachedEmbedding> Embeddings { get; set; } = new();
}

public class EmbeddingCacheService
{
    private readonly string _cacheDirectory;

    public EmbeddingCacheService()
    {
        // Usar directorio de datos del usuario para el caché
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChurchPresenter",
            "Cache",
            "Embeddings"
        );
        _cacheDirectory = appDataPath;
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task SaveCacheAsync(string bibleVersion, List<IndexedVerse> verses)
    {
        var cache = new EmbeddingCache
        {
            Version = bibleVersion,
            CreatedAt = DateTime.UtcNow,
            Embeddings = verses.Select(v => new CachedEmbedding
            {
                Key = $"{v.Book}_{v.Chapter}_{v.VerseNumber}",
                Embedding = v.Embedding
            }).ToList()
        };

        var fileName = GetCacheFileName(bibleVersion);
        var json = JsonSerializer.Serialize(cache);
        await File.WriteAllTextAsync(fileName, json);
        
        Console.WriteLine($"Caché guardada: {fileName} ({cache.Embeddings.Count} versículos)");
    }

    public async Task<Dictionary<string, float[]>?> LoadCacheAsync(string bibleVersion)
    {
        var fileName = GetCacheFileName(bibleVersion);
        
        if (!File.Exists(fileName))
        {
            Console.WriteLine($"No existe caché para {bibleVersion}");
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(fileName);
            var cache = JsonSerializer.Deserialize<EmbeddingCache>(json);
            
            if (cache == null)
                return null;

            Console.WriteLine($"Caché cargada: {fileName} ({cache.Embeddings.Count} versículos, creada: {cache.CreatedAt})");
            
            return cache.Embeddings.ToDictionary(
                e => e.Key,
                e => e.Embedding
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar caché: {ex.Message}");
            return null;
        }
    }

    public bool HasCache(string bibleVersion)
    {
        var fileName = GetCacheFileName(bibleVersion);
        return File.Exists(fileName);
    }

    public void ClearCache(string? bibleVersion = null)
    {
        if (bibleVersion != null)
        {
            var fileName = GetCacheFileName(bibleVersion);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                Console.WriteLine($"Caché eliminada: {fileName}");
            }
        }
        else
        {
            // Eliminar todas las cachés
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
                Console.WriteLine("Todas las cachés eliminadas");
            }
        }
    }

    private string GetCacheFileName(string bibleVersion)
    {
        // Sanitizar el nombre del archivo
        var safeFileName = string.Join("_", bibleVersion.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{safeFileName}_embeddings.json");
    }
}
