using System.Collections.Generic;

namespace ChurchPresenter.Models;

public enum VerseType
{
    Verse,
    Chorus,
    Bridge,
    PreChorus
}

public class Verse
{
    public string Content { get; set; } = string.Empty;
    public VerseType Type { get; set; }
    public int Order { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class Song
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<Verse> Verses { get; set; } = new();
    public string? Notes { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
