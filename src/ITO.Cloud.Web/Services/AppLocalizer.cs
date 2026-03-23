using System.Text.Json;
using Microsoft.JSInterop;

namespace ITO.Cloud.Web.Services;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string Lang { get; }
    string[] SupportedLanguages { get; }
    string GetLanguageDisplayName(string lang);
    string GetLanguageFlag(string lang);
    Task InitializeAsync();
    Task SetLanguageAsync(string lang);
    event Action? OnChange;
}

public class AppLocalizer : IAppLocalizer
{
    private readonly IWebHostEnvironment _env;
    private readonly IJSRuntime _js;
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _currentLang = "es";
    private bool _initialized;

    public static readonly string[] Languages = ["es", "en", "pt"];

    private static readonly Dictionary<string, string> LanguageNames = new()
    {
        ["es"] = "Español",
        ["en"] = "English",
        ["pt"] = "Português"
    };

    private static readonly Dictionary<string, string> LanguageFlags = new()
    {
        ["es"] = "\ud83c\uddea\ud83c\uddf8",
        ["en"] = "\ud83c\uddfa\ud83c\uddf8",
        ["pt"] = "\ud83c\udde7\ud83c\uddf7"
    };

    public AppLocalizer(IWebHostEnvironment env, IJSRuntime js)
    {
        _env = env;
        _js = js;
    }

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(_currentLang, out var dict) && dict.TryGetValue(key, out var val))
                return val;
            if (_translations.TryGetValue("es", out var fallback) && fallback.TryGetValue(key, out var fbVal))
                return fbVal;
            return key;
        }
    }

    public string Lang => _currentLang;
    public string[] SupportedLanguages => Languages;

    public string GetLanguageDisplayName(string lang) =>
        LanguageNames.GetValueOrDefault(lang, lang);

    public string GetLanguageFlag(string lang) =>
        LanguageFlags.GetValueOrDefault(lang, "");

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        foreach (var lang in Languages)
        {
            var path = Path.Combine(_env.WebRootPath, "locales", $"{lang}.json");
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict != null)
                    _translations[lang] = dict;
            }
        }

        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", "ito-lang");
            if (!string.IsNullOrEmpty(stored) && _translations.ContainsKey(stored))
                _currentLang = stored;
        }
        catch
        {
            // JS interop may fail during prerendering
        }

        _initialized = true;
    }

    public async Task SetLanguageAsync(string lang)
    {
        if (!_translations.ContainsKey(lang)) return;
        _currentLang = lang;

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "ito-lang", lang);
        }
        catch { }

        OnChange?.Invoke();
    }
}
