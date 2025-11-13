using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ChurchPresenter.Models;

namespace ChurchPresenter.Services;

public class SongService
{
    private readonly string _songsDirectory;

    public SongService()
    {
        // Usar directorio de datos del usuario en lugar del directorio de la aplicación
        // Esto permite que funcione correctamente en AppImages y otras instalaciones portables
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ChurchPresenter",
            "Songs"
        );
        _songsDirectory = appDataPath;
        Directory.CreateDirectory(_songsDirectory);
    }

    public async Task<List<Song>> LoadAllSongsAsync()
    {
        var songs = new List<Song>();
        foreach (var file in Directory.GetFiles(_songsDirectory, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var song = JsonSerializer.Deserialize<Song>(json);
                if (song != null)
                {
                    song.FilePath = file;
                    songs.Add(song);
                }
            }
            catch (Exception ex)
            {
                // Log error or handle invalid files
                Console.WriteLine($"Error loading song from {file}: {ex.Message}");
            }
        }
        return songs;
    }

    public async Task SaveSongAsync(Song song)
    {
        if (string.IsNullOrEmpty(song.FilePath))
        {
            song.FilePath = Path.Combine(_songsDirectory, $"{song.Title.Replace(" ", "_")}.json");
        }

        var json = JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(song.FilePath, json);
    }
}