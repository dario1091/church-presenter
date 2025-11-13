using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchPresenter.Models;

namespace ChurchPresenter.Services;

public class VerseSearchResult
{
    public string Book { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int VerseNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string BibleVersion { get; set; } = string.Empty;
    public float Similarity { get; set; }
    public string Citation => $"{Book} {Chapter}:{VerseNumber} ({BibleVersion})";
}

public class IndexedVerse
{
    public string Book { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int VerseNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string BibleVersion { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

public class SemanticSearchService : IDisposable
{
    private readonly EmbeddingService _embeddingService;
    private readonly EmbeddingCacheService _cacheService;
    private readonly List<IndexedVerse> _indexedVerses;
    private bool _isIndexed;
    private string? _currentBibleVersion; // Track cual Biblia está indexada

    public event Action<int, int>? IndexingProgress; // Current, Total

    public SemanticSearchService()
    {
        _embeddingService = new EmbeddingService();
        _cacheService = new EmbeddingCacheService();
        _indexedVerses = new List<IndexedVerse>();
        _isIndexed = false;
        _currentBibleVersion = null;
    }

    public async Task IndexBibleAsync(Bible bible)
    {
        var bibleVersion = bible.FileCode ?? bible.Name;
        
        // Si ya está indexada esta misma Biblia, no hacer nada
        if (_isIndexed && _currentBibleVersion == bibleVersion)
        {
            Console.WriteLine($"La Biblia {bibleVersion} ya está indexada en memoria. Usando índice existente.");
            return;
        }
        
        // Si hay una Biblia diferente indexada, limpiar primero
        if (_isIndexed && _currentBibleVersion != bibleVersion)
        {
            Console.WriteLine($"Limpiando índice de {_currentBibleVersion} para cargar {bibleVersion}");
            ClearIndex();
        }
        
        // Intentar cargar desde caché primero
        var cachedEmbeddings = await _cacheService.LoadCacheAsync(bibleVersion);
        
        if (cachedEmbeddings != null)
        {
            Console.WriteLine($"Usando caché existente para {bibleVersion}");
            await LoadFromCacheAsync(bible, cachedEmbeddings);
            _isIndexed = true;
            _currentBibleVersion = bibleVersion;
            return;
        }

        // Si no hay caché, indexar desde cero
        Console.WriteLine($"No hay caché para {bibleVersion}, indexando...");
        await IndexFromScratchAsync(bible);
        
        // Guardar en caché
        await _cacheService.SaveCacheAsync(bibleVersion, _indexedVerses);
        _isIndexed = true;
        _currentBibleVersion = bibleVersion;
    }

    private async Task LoadFromCacheAsync(Bible bible, Dictionary<string, float[]> cachedEmbeddings)
    {
        await Task.Run(() =>
        {
            int loaded = 0;
            foreach (var testament in bible.Testaments.Values)
            {
                foreach (var book in testament.Books.Values)
                {
                    foreach (var chapter in book.Chapters.Values)
                    {
                        var versesDictionary = chapter.GetVersesDictionary();
                        foreach (var verse in versesDictionary)
                        {
                            var chapterNum = int.TryParse(chapter.Number, out var ch) ? ch : 0;
                            var verseNum = int.TryParse(verse.Key, out var v) ? v : 0;
                            var key = $"{book.Name}_{chapterNum}_{verseNum}";
                            
                            if (cachedEmbeddings.TryGetValue(key, out var embedding))
                            {
                                _indexedVerses.Add(new IndexedVerse
                                {
                                    Book = book.Name,
                                    Chapter = chapterNum,
                                    VerseNumber = verseNum,
                                    Text = verse.Value,
                                    BibleVersion = bible.FileCode ?? "Desconocida",
                                    Embedding = embedding
                                });
                                loaded++;
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Cargados {loaded} versículos desde caché");
        });
    }

    private async Task IndexFromScratchAsync(Bible bible)
    {
        await Task.Run(() =>
        {
            // Primero contar el total de versículos
            int totalVerses = 0;
            foreach (var testament in bible.Testaments.Values)
            {
                foreach (var book in testament.Books.Values)
                {
                    foreach (var chapter in book.Chapters.Values)
                    {
                        totalVerses += chapter.GetVersesDictionary().Count;
                    }
                }
            }

            Console.WriteLine($"Iniciando indexación de {totalVerses} versículos...");
            int processedVerses = 0;

            foreach (var testament in bible.Testaments.Values)
            {
                foreach (var book in testament.Books.Values)
                {
                    Console.WriteLine($"Indexando libro: {book.Name}");
                    foreach (var chapter in book.Chapters.Values)
                    {
                        var versesDictionary = chapter.GetVersesDictionary();
                        foreach (var verse in versesDictionary)
                        {
                            var verseText = verse.Value;
                            if (string.IsNullOrWhiteSpace(verseText))
                                continue;

                            var embedding = _embeddingService.GenerateEmbedding(verseText);
                            
                            _indexedVerses.Add(new IndexedVerse
                            {
                                Book = book.Name,
                                Chapter = int.TryParse(chapter.Number, out var chapterNum) ? chapterNum : 0,
                                VerseNumber = int.TryParse(verse.Key, out var verseNum) ? verseNum : 0,
                                Text = verseText,
                                BibleVersion = bible.FileCode ?? "Desconocida",
                                Embedding = embedding
                            });

                            processedVerses++;
                            
                            // Reportar progreso cada 100 versículos
                            if (processedVerses % 100 == 0)
                            {
                                IndexingProgress?.Invoke(processedVerses, totalVerses);
                                Console.WriteLine($"Progreso: {processedVerses}/{totalVerses} versículos ({(processedVerses * 100 / totalVerses)}%)");
                            }
                        }
                    }
                }
            }
            
            // Reportar progreso final
            IndexingProgress?.Invoke(totalVerses, totalVerses);
            Console.WriteLine($"Indexación completada: {totalVerses} versículos");
        });
        
        _isIndexed = true;
    }

    public async Task<List<VerseSearchResult>> SearchAsync(string query, int topK = 10)
    {
        if (!_isIndexed)
            throw new InvalidOperationException("El índice no ha sido creado. Llama a IndexBibleAsync primero.");

        if (string.IsNullOrWhiteSpace(query))
            return new List<VerseSearchResult>();

        return await Task.Run(() =>
        {
            Console.WriteLine($"Buscando: '{query}'");
            
            // Expandir y mejorar la consulta
            var enhancedQuery = EnhanceQuery(query);
            Console.WriteLine($"Consulta mejorada: '{enhancedQuery}'");
            
            // Generar embedding de la consulta mejorada
            var queryEmbedding = _embeddingService.GenerateEmbedding(enhancedQuery);
            Console.WriteLine($"Embedding generado con {queryEmbedding.Length} dimensiones");

            // Calcular similitud con todos los versículos
            var results = _indexedVerses
                .Select(verse => new VerseSearchResult
                {
                    Book = verse.Book,
                    Chapter = verse.Chapter,
                    VerseNumber = verse.VerseNumber,
                    Text = verse.Text,
                    BibleVersion = verse.BibleVersion,
                    Similarity = EmbeddingService.CosineSimilarity(queryEmbedding, verse.Embedding)
                })
                .OrderByDescending(r => r.Similarity)
                .Take(topK)
                .ToList();

            // Mostrar los top 3 resultados en consola
            Console.WriteLine($"Top 3 resultados:");
            foreach (var result in results.Take(3))
            {
                Console.WriteLine($"  - {result.Citation} ({result.Similarity:P1}): {result.Text.Substring(0, Math.Min(50, result.Text.Length))}...");
            }

            return results;
        });
    }

    private string EnhanceQuery(string query)
    {
        // Normalizar el texto (minúsculas)
        query = query.ToLowerInvariant();
        
        // Expandir palabras comunes con sinónimos bíblicos
        var synonyms = new Dictionary<string, string[]>
        {
            { "dios", new[] { "señor", "jehová", "yahweh", "padre celestial", "todopoderoso" } },
            { "jesús", new[] { "cristo", "hijo de dios", "salvador", "mesías", "cordero" } },
            { "espíritu", new[] { "espíritu santo", "consolador", "paráclito" } },
            { "salvación", new[] { "salvación", "redención", "liberación" } },
            { "fe", new[] { "fe", "confianza", "creencia" } },
            { "amor", new[] { "amor", "caridad", "misericordia" } },
            { "paz", new[] { "paz", "tranquilidad", "reposo" } },
            { "fuerza", new[] { "fuerza", "fortaleza", "poder", "potencia" } },
            { "gracia", new[] { "gracia", "favor", "misericordia" } },
            { "vida", new[] { "vida", "existencia" } },
            { "muerte", new[] { "muerte", "fallecimiento" } },
            { "cielo", new[] { "cielo", "paraíso", "gloria" } },
            { "pecado", new[] { "pecado", "transgresión", "iniquidad" } },
            { "perdón", new[] { "perdón", "remisión" } }
        };
        
        // No expandir si la query es muy larga (probablemente sea una cita exacta)
        if (query.Split(' ').Length > 10)
            return query;
        
        // Mantener la query original como principal
        return query;
    }

    public bool IsIndexed => _isIndexed;

    public int IndexedVersesCount => _indexedVerses.Count;

    public void ClearIndex()
    {
        _indexedVerses.Clear();
        _isIndexed = false;
        _currentBibleVersion = null;
    }

    public void Dispose()
    {
        _embeddingService?.Dispose();
        _indexedVerses.Clear();
    }
}
