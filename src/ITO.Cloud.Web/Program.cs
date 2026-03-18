using ITO.Cloud.Web.Auth;
using ITO.Cloud.Web.Components;
using ITO.Cloud.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ── HttpContextAccessor ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Blazor Server ────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MudBlazor ────────────────────────────────────────────────────────────────
builder.Services.AddMudServices();

// ── Auth ─────────────────────────────────────────────────────────────────────
// Handler de paso — registra IAuthenticationService sin bloquear rutas Blazor.
// La autenticación real la gestiona JwtAuthStateProvider con ProtectedSessionStorage.
builder.Services.AddAuthentication("Passthrough")
    .AddScheme<AuthenticationSchemeOptions, PassthroughAuthHandler>("Passthrough", _ => { });
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

// ── HttpClient → API ─────────────────────────────────────────────────────────
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7200";
builder.Services.AddScoped<ApiAuthService>();
builder.Services.AddHttpClient<ItoApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout     = TimeSpan.FromSeconds(5);   // evita esperas largas si la API no arrancó aún
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
