using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChurchPresenter.Models;

public class Bible
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public string Order { get; set; } = "999";

    // File code (filename without extension) used to map the JSON file to the model
    [JsonIgnore]
    public string FileCode { get; set; } = string.Empty;

    [JsonPropertyName("testaments")]
    public Dictionary<string, Testament> Testaments { get; set; } = new();
}

public class Testament
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("books")]
    public Dictionary<string, Book> Books { get; set; } = new();
}

public class Book
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("chapters")]
    public Dictionary<string, Chapter> Chapters { get; set; } = new();
}

public class Chapter
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("verses")]
    public Dictionary<string, JsonElement> Verses { get; set; } = new();

    public Dictionary<string, string> GetVersesDictionary()
    {
        var result = new Dictionary<string, string>();
        foreach (var verse in Verses)
        {
            try
            {
                if (verse.Value.ValueKind == JsonValueKind.String)
                {
                    result[verse.Key] = verse.Value.GetString() ?? string.Empty;
                }
                else if (verse.Value.ValueKind == JsonValueKind.Object)
                {
                    // Si el verso es un objeto, intentamos obtener la propiedad "text"
                    if (verse.Value.TryGetProperty("text", out var textElement) && 
                        textElement.ValueKind == JsonValueKind.String)
                    {
                        result[verse.Key] = textElement.GetString() ?? string.Empty;
                    }
                    else
                    {
                        // Buscar cualquier propiedad que contenga un string
                        foreach (var prop in verse.Value.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                result[verse.Key] = prop.Value.GetString() ?? string.Empty;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar versículo {verse.Key}: {ex.Message}");
                result[verse.Key] = $"[Error al cargar versículo {verse.Key}]";
            }
        }
        return result;
    }
}