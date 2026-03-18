using ITO.Cloud.API.Middleware;
using ITO.Cloud.Application;
using ITO.Cloud.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

// ── Serilog bootstrap ─────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog desde appsettings
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    // ── Capas de la aplicación ────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience            = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew                = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization();

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowBlazor", policy =>
            policy.WithOrigins("https://localhost:5001", "http://localhost:5001",
                               "https://localhost:7001", "http://localhost:7001",
                               "http://localhost:5047", "https://localhost:5047")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "ITO Cloud API",
            Version     = "v1",
            Description = "Plataforma de Inspección Técnica de Obras"
        });

        // JWT en Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.ApiKey,
            Scheme       = "Bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Ingrese: Bearer {su-token}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ITO Cloud API v1");
            c.RoutePrefix = string.Empty;   // Swagger en raíz /
        });
    }

    app.UseCors("AllowBlazor");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Health check ──────────────────────────────────────────────────────────
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

    // ── Seed de usuarios (solo Development) ───────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.MapPost("/dev/seed-users", async (
            Microsoft.AspNetCore.Identity.UserManager<ITO.Cloud.Domain.Entities.Identity.ApplicationUser> userManager,
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>> roleManager) =>
        {
            // Roles del sistema
            string[] roles = ["SuperAdmin", "AdminTenant", "Director", "Supervisor", "Inspector", "Contratista"];
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>(role));

            // Reset hash admin
            var admin = await userManager.FindByEmailAsync("admin@itocloud.cl");
            if (admin != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                var r = await userManager.ResetPasswordAsync(admin, token, "Admin123!");
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
                return Results.Ok(new { ok = r.Succeeded, errors = r.Errors.Select(e => e.Description) });
            }
            return Results.NotFound("Admin user not found");
        });
    }

    Log.Information("ITO Cloud API iniciando en {Env}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
