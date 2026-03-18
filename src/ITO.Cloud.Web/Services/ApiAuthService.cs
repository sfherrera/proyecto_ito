using ITO.Cloud.Web.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace ITO.Cloud.Web.Services;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt, UserInfo User);
public record UserInfo(string Id, string Email, string FullName, string TenantId, List<string> Roles);

public class ApiAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly TokenStorageService _tokenStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IConfiguration _config;

    public ApiAuthService(
        IHttpClientFactory clientFactory,
        TokenStorageService tokenStorage,
        AuthenticationStateProvider authStateProvider,
        IConfiguration config)
    {
        _clientFactory     = clientFactory;
        _tokenStorage      = tokenStorage;
        _authStateProvider = authStateProvider;
        _config            = config;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        var apiUrl = _config["ApiSettings:BaseUrl"] ?? "https://localhost:7200";
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(apiUrl);

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
            if (!response.IsSuccessStatusCode)
                return (false, "Credenciales incorrectas.");

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null) return (false, "Respuesta inválida del servidor.");

            await _tokenStorage.SetTokenAsync(result.Token);

            if (_authStateProvider is JwtAuthStateProvider jwtProvider)
                jwtProvider.NotifyAuthStateChanged();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearAsync();
        if (_authStateProvider is JwtAuthStateProvider jwtProvider)
            jwtProvider.NotifyAuthStateChanged();
    }
}
