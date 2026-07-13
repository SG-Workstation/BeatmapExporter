using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace BeatmapExporterCore.Localization;

public class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    private Dictionary<string, string> _strings = new();
    private readonly List<LocaleInfo> _availableLocales = new();
    private LocaleInfo _currentLocale;

    private LocalizationService()
    {
        DiscoverLocales();
        _currentLocale = _availableLocales.FirstOrDefault() ?? new LocaleInfo("English", "en", null);
        LoadLocale(_currentLocale);
    }

    private void DiscoverLocales()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        var localeFiles = resourceNames
            .Where(r => r.StartsWith("BeatmapExporterCore.Localization.Locales.") && r.EndsWith(".json"))
            .ToList();

        foreach (var resource in localeFiles)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resource);
                if (stream is null) continue;
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                using var doc = JsonDocument.Parse(json);
                var language = doc.RootElement.TryGetProperty("language", out var lang) ? lang.GetString() ?? "Unknown" : "Unknown";
                var culture = doc.RootElement.TryGetProperty("culture", out var cult) ? cult.GetString() ?? "" : "";
                _availableLocales.Add(new LocaleInfo(language, culture, resource));
            }
            catch
            {
                // Skip invalid locale files
            }
        }

        _availableLocales.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    private void LoadLocale(LocaleInfo locale)
    {
        _strings.Clear();

        if (locale.ResourcePath is null) return;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(locale.ResourcePath);
            if (stream is null) return;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            using var doc = JsonDocument.Parse(json);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name is "language" or "culture") continue;
                _strings[prop.Name] = prop.Value.GetString() ?? prop.Name;
            }
        }
        catch
        {
            // Fall back to empty strings
        }
    }

    public IReadOnlyList<LocaleInfo> AvailableLocales => _availableLocales;

    public LocaleInfo CurrentLocale
    {
        get => _currentLocale;
        set
        {
            if (value.Equals(_currentLocale)) return;
            _currentLocale = value;
            LoadLocale(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        }
    }

    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : key;

    public string Format(string key, params object?[] args) =>
        string.Format(this[key], args);

    public event PropertyChangedEventHandler? PropertyChanged;
}
