namespace BeatmapExporterCore.Localization;

public class LocaleInfo
{
    public string Name { get; }
    public string Culture { get; }
    public string? ResourcePath { get; }

    internal LocaleInfo(string name, string culture, string? resourcePath)
    {
        Name = name;
        Culture = culture;
        ResourcePath = resourcePath;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) =>
        obj is LocaleInfo other && other.Culture == Culture;

    public override int GetHashCode() => Culture.GetHashCode();
}
