using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace ITO.Cloud.Web.Auth;

/// <summary>
/// Handler de autenticación de paso — nunca bloquea peticiones HTTP.
/// La autenticación real la gestiona JwtAuthStateProvider dentro de Blazor.
/// Necesario para registrar IAuthenticationService sin interceptar rutas.
/// </summary>
public class PassthroughAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // No-op: deja que Blazor maneje la redirección a /login
        return Task.CompletedTask;
    }
}
