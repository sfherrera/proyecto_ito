using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace ITO.Cloud.Web.Auth;

/// <summary>
/// Almacena el JWT en el sessionStorage del navegador (encriptado server-side).
/// Funciona en modo Interactive Server — no requiere ISession.
/// </summary>
public class TokenStorageService
{
    private const string TokenKey = "ito_jwt";
    private const string UserKey  = "ito_user";

    private readonly ProtectedSessionStorage _storage;

    // Cache en memoria para acceso síncrono
    private string? _cachedToken;
    private string? _cachedUser;

    public TokenStorageService(ProtectedSessionStorage storage)
        => _storage = storage;

    public async Task SetTokenAsync(string token)
    {
        _cachedToken = token;
        await _storage.SetAsync(TokenKey, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken is not null)
            return _cachedToken;

        try
        {
            var result = await _storage.GetAsync<string>(TokenKey);
            _cachedToken = result.Success ? result.Value : null;
            return _cachedToken;
        }
        catch { return null; }
    }

    // Versión síncrona para JwtAuthStateProvider (usa cache)
    public string? GetToken() => _cachedToken;

    public async Task SetUserAsync(string userJson)
    {
        _cachedUser = userJson;
        await _storage.SetAsync(UserKey, userJson);
    }

    public async Task<string?> GetUserAsync()
    {
        if (_cachedUser is not null)
            return _cachedUser;

        try
        {
            var result = await _storage.GetAsync<string>(UserKey);
            _cachedUser = result.Success ? result.Value : null;
            return _cachedUser;
        }
        catch { return null; }
    }

    public async Task ClearAsync()
    {
        _cachedToken = null;
        _cachedUser  = null;
        try { await _storage.DeleteAsync(TokenKey); } catch { /* ignorar */ }
        try { await _storage.DeleteAsync(UserKey); } catch { /* ignorar */ }
    }

    public bool IsAuthenticated => _cachedToken is not null;
}
