using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ITO.Cloud.Web.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(TokenStorageService tokenStorage)
        => _tokenStorage = tokenStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Anonymous;

        try
        {
            var handler  = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await _tokenStorage.ClearAsync();
                return Anonymous;
            }

            var claims    = jwtToken.Claims.ToList();
            var identity  = new ClaimsIdentity(claims, "jwt", JwtRegisteredClaimNames.Email, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        catch
        {
            await _tokenStorage.ClearAsync();
            return Anonymous;
        }
    }

    public void NotifyAuthStateChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
