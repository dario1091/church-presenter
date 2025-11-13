using System;
using System.Collections.Generic;
using System.Linq;
using ChurchPresenter.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChurchPresenter.ViewModels;

public partial class EditSongViewModel : ViewModelBase
{
    private readonly Song _originalSong;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string author = string.Empty;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private string lyricsText = string.Empty;

    public EditSongViewModel(Song? song = null)
    {
        _originalSong = song ?? new Song();
        Title = _originalSong.Title;
        Author = _originalSong.Author;
        Notes = _originalSong.Notes;
        
        // Convertir los versos existentes al formato de texto
        if (_originalSong.Verses.Any())
        {
            ConvertVersesToText(_originalSong.Verses);
        }
    }

    private void ConvertVersesToText(List<Verse> verses)
    {
        var text = string.Empty;
        foreach (var verse in verses.OrderBy(v => v.Order))
        {
            var header = verse.Type switch
            {
                VerseType.Verse => verse.Label?.StartsWith("Verso", StringComparison.OrdinalIgnoreCase) == true 
                    ? verse.Label.ToUpper() 
                    : $"VERSE {RomanNumeral(verses.Count(v => v.Type == VerseType.Verse && v.Order < verse.Order) + 1)}",
                VerseType.Chorus => "CHORUS",
                VerseType.Bridge => "BRIDGE",
                VerseType.PreChorus => "PRE-CHORUS",
                _ => verse.Label?.ToUpper() ?? $"PART {verse.Order + 1}"
            };

            text += $"{header}\n{verse.Content}\n\n";
        }
        LyricsText = text.TrimEnd();
    }

    private string RomanNumeral(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            _ => number.ToString()
        };
    }

    private List<Verse> ParseVerses()
    {
        var verses = new List<Verse>();
        var lines = LyricsText.Split('\n');
        
        Verse? currentVerse = null;
        var verseCount = 0;
        var order = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine)) continue;

            if (IsVerseHeader(trimmedLine, out var type, out var label))
            {
                if (currentVerse != null)
                {
                    currentVerse.Content = currentVerse.Content.TrimEnd();
                    verses.Add(currentVerse);
                }

                if (type == VerseType.Verse) verseCount++;

                currentVerse = new Verse
                {
                    Type = type,
                    Label = label ?? GetDefaultLabel(type, verseCount),
                    Order = order++,
                    Content = string.Empty
                };
            }
            else if (currentVerse != null)
            {
                currentVerse.Content += (currentVerse.Content.Length > 0 ? "\n" : "") + trimmedLine;
            }
        }

        if (currentVerse != null)
        {
            currentVerse.Content = currentVerse.Content.TrimEnd();
            verses.Add(currentVerse);
        }

        return verses;
    }

    private bool IsVerseHeader(string line, out VerseType type, out string? label)
    {
        line = line.ToUpper().Trim();
        type = VerseType.Verse;
        label = null;

        if (line.StartsWith("VERSE ") || line.StartsWith("VERSO "))
        {
            type = VerseType.Verse;
            label = line;
            return true;
        }
        
        if (line is "CHORUS" or "CORO")
        {
            type = VerseType.Chorus;
            label = "Coro";
            return true;
        }
        
        if (line is "BRIDGE" or "PUENTE")
        {
            type = VerseType.Bridge;
            label = "Puente";
            return true;
        }
        
        if (line is "PRE-CHORUS" or "PRE-CORO")
        {
            type = VerseType.PreChorus;
            label = "Pre-Coro";
            return true;
        }

        return false;
    }

    private string GetDefaultLabel(VerseType type, int count)
    {
        return type switch
        {
            VerseType.Verse => $"Verso {count}",
            VerseType.Chorus => "Coro",
            VerseType.Bridge => "Puente",
            VerseType.PreChorus => "Pre-Coro",
            _ => $"Parte {count}"
        };
    }

    public Song Save()
    {
        _originalSong.Title = Title;
        _originalSong.Author = Author;
        _originalSong.Notes = Notes;
        _originalSong.Verses = ParseVerses();
        return _originalSong;
    }
}