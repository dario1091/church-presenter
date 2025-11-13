using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChurchPresenter.Models;

namespace ChurchPresenter.Services;

public class BibleService
{
    private readonly string _biblesPath;
    private readonly Dictionary<string, Bible> _bibleCache;

    public BibleService()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _biblesPath = Path.Combine(baseDir, "Assets", "Bibles");
        _bibleCache = new Dictionary<string, Bible>();

        Console.WriteLine($"Base directory: {baseDir}");
        Console.WriteLine($"Bibles path: {_biblesPath}");
    }

    public async Task<List<Bible>> GetBiblesAsync()
    {
        Console.WriteLine($"Searching for Bibles in: {_biblesPath}");
        var bibles = new List<Bible>();

        if (!Directory.Exists(_biblesPath))
        {
            Console.WriteLine("Bibles directory does not exist!");
            return bibles;
        }

        var files = Directory.GetFiles(_biblesPath, "*.json");
        Console.WriteLine($"Found {files.Length} Bible files");

        foreach (var file in files)
        {
            try
            {
                var bibleCode = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine($"Loading Bible: {bibleCode}");
                var bible = await LoadBibleAsync(bibleCode);
                if (bible != null)
                {
                    Console.WriteLine($"Successfully loaded Bible: {bible.Name}");
                    bibles.Add(bible);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Bible from {file}: {ex.Message}");
            }
        }

        Console.WriteLine($"Total Bibles loaded: {bibles.Count}");
        
        // Ordenar las biblias por el campo Order
        return bibles.OrderBy(b => int.TryParse(b.Order, out var order) ? order : 999).ToList();
    }

    private async Task<Bible?> LoadBibleAsync(string bibleCode)
    {
        try
        {
            Console.WriteLine($"Attempting to load Bible: {bibleCode}");

            if (_bibleCache.TryGetValue(bibleCode, out var cachedBible))
            {
                Console.WriteLine("Bible found in cache");
                return cachedBible;
            }

            var filePath = Path.Combine(_biblesPath, $"{bibleCode}.json");
            Console.WriteLine($"Looking for Bible file at: {filePath}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Bible file not found at {filePath}");
                if (Directory.Exists(_biblesPath))
                {
                    var files = Directory.GetFiles(_biblesPath);
                    Console.WriteLine($"Files in directory: {string.Join(", ", files)}");
                }
                return null;
            }

            Console.WriteLine($"Reading Bible file from {filePath}...");
            var jsonContent = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrEmpty(jsonContent))
            {
                Console.WriteLine("Bible file is empty");
                return null;
            }

            Console.WriteLine($"File content length: {jsonContent.Length} bytes");
            Console.WriteLine("Attempting to deserialize Bible file...");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var bible = JsonSerializer.Deserialize<Bible>(jsonContent, options);

            if (bible == null)
            {
                Console.WriteLine("Failed to deserialize Bible - result was null");
                return null;
            }

            if (string.IsNullOrEmpty(bible.Version))
            {
                bible.Version = bibleCode;
            }

            if (string.IsNullOrEmpty(bible.Name))
            {
                bible.Name = bibleCode;
            }

            if (bible.Testaments == null || bible.Testaments.Count == 0)
            {
                Console.WriteLine("Warning: Bible has no testaments");
                return null;
            }

            Console.WriteLine($"Bible loaded successfully:");
            Console.WriteLine($"- Name: {bible.Name}");
            Console.WriteLine($"- Version: {bible.Version}");
            Console.WriteLine($"- Language: {bible.Language}");
            Console.WriteLine($"- Number of testaments: {bible.Testaments.Count}");
            
            foreach (var testament in bible.Testaments)
            {
                Console.WriteLine($"- Testament {testament.Key}: {testament.Value.Books.Count} books");
            }
            // store the file code so we can map the loaded Bible back to the source file name
            bible.FileCode = bibleCode;
            _bibleCache[bibleCode] = bible;
            return bible;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing Bible {bibleCode}:");
            Console.WriteLine($"- Message: {ex.Message}");
            Console.WriteLine($"- Line Number: {ex.LineNumber}");
            Console.WriteLine($"- Path: {ex.Path}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error loading Bible {bibleCode}:");
            Console.WriteLine($"- Type: {ex.GetType().Name}");
            Console.WriteLine($"- Message: {ex.Message}");
            Console.WriteLine($"- Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<(string? title, Dictionary<string, string>? verses)> GetChapterAsync(string bibleCode, string bookName, string chapterNumber)
    {
        Console.WriteLine($"GetChapterAsync - Bible: {bibleCode}, Book: {bookName}, Chapter: {chapterNumber}");

        // Try to resolve the bible from cache by FileCode, Version or Name
        var bible = _bibleCache.Values.FirstOrDefault(b =>
            string.Equals(b.FileCode, bibleCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(b.Version, bibleCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(b.Name, bibleCode, StringComparison.OrdinalIgnoreCase)
        );

        // If not found in cache, try loading by file code (filename without extension)
        if (bible == null)
        {
            var fileCode = Path.GetFileNameWithoutExtension(bibleCode);
            bible = await LoadBibleAsync(fileCode);
        }

        if (bible == null)
        {
            Console.WriteLine("Bible not found in cache or could not be loaded");
            return (null, null);
        }

        Console.WriteLine($"Found Bible: {bible.Name}");
        foreach (var testament in bible.Testaments.Values)
        {
            Console.WriteLine($"Checking testament: {testament.Name}");
            foreach (var book in testament.Books.Values)
            {
                Console.WriteLine($"Checking book: {book.Name}");
                if (book.Name.Equals(bookName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Found matching book: {book.Name}");
                    if (book.Chapters.TryGetValue(chapterNumber, out var chapter))
                    {
                        var title = $"{book.Name} {chapterNumber}";
                        var verses = chapter.GetVersesDictionary();
                        Console.WriteLine($"Found chapter: {title} with {verses.Count} verses");
                        return (title, verses);
                    }
                    Console.WriteLine($"Chapter {chapterNumber} not found in book {book.Name}");
                    return (null, null);
                }
            }
        }

        Console.WriteLine($"Book {bookName} not found in Bible {bibleCode}");
        return (null, null);
    }
}