using ITO.Cloud.API.Middleware;
using ITO.Cloud.Application;
using ITO.Cloud.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
            Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>> roleManager,
            ITO.Cloud.Infrastructure.Persistence.ApplicationDbContext db) =>
        {
          try {
            var results = new List<object>();
            const string password = "Test1234!";

            // 1. Crear roles del sistema (limpiar duplicados primero)
            string[] roles = ["SuperAdmin", "AdminTenant", "Director", "Supervisor", "Inspector", "Contratista"];
            foreach (var role in roles)
            {
                // Eliminar roles duplicados si existen (del SQL seed)
                var duplicates = await db.Roles
                    .Where(r => r.NormalizedName == role.ToUpperInvariant())
                    .OrderBy(r => r.Id)
                    .ToListAsync();
                if (duplicates.Count > 1)
                {
                    // Mantener solo el primero, eliminar el resto
                    for (int i = 1; i < duplicates.Count; i++)
                        db.Roles.Remove(duplicates[i]);
                    await db.SaveChangesAsync();
                }
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>(role));
            }

            // 2. Asegurar tenants existen
            var tenant1Id = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var tenant2Id = Guid.Parse("22222222-0000-0000-0000-000000000001");

            if (await db.Tenants.FindAsync(tenant1Id) == null)
            {
                db.Tenants.Add(new ITO.Cloud.Domain.Entities.Identity.Tenant
                {
                    Id = tenant1Id, Name = "ITO Pacífico SpA", Slug = "ito-pacifico",
                    Plan = "professional", IsActive = true, PrimaryColor = "#1F3864",
                    MaxUsers = 20, MaxProjects = 15
                });
            }
            if (await db.Tenants.FindAsync(tenant2Id) == null)
            {
                db.Tenants.Add(new ITO.Cloud.Domain.Entities.Identity.Tenant
                {
                    Id = tenant2Id, Name = "Consultores Sur Ltda", Slug = "consultores-sur",
                    Plan = "basic", IsActive = true, PrimaryColor = "#1A6B3C",
                    MaxUsers = 8, MaxProjects = 5
                });
            }
            await db.SaveChangesAsync();

            // 3. Definir usuarios de prueba (IDs fijos para seed-data)
            var testUsers = new[]
            {
                new { Id = Guid.Parse("11111111-1111-0000-0000-000000000001"), Email = "admin@itopacifico.cl", UserName = "admin.pacifico", FirstName = "Rodrigo", LastName = "Fuentes Vidal", Rut = "12.345.678-9", Position = "Administrador ITO", TenantId = tenant1Id, Roles = new[] { "AdminTenant", "SuperAdmin" } },
                new { Id = Guid.Parse("11111111-1111-0000-0000-000000000002"), Email = "carlos.mendez@itopacifico.cl", UserName = "carlos.mendez", FirstName = "Carlos", LastName = "Méndez Torres", Rut = "13.456.789-0", Position = "Supervisor de Obra", TenantId = tenant1Id, Roles = new[] { "Supervisor" } },
                new { Id = Guid.Parse("11111111-1111-0000-0000-000000000003"), Email = "pablo.rojas@itopacifico.cl", UserName = "pablo.rojas", FirstName = "Pablo", LastName = "Rojas Soto", Rut = "14.567.890-1", Position = "Inspector ITO Senior", TenantId = tenant1Id, Roles = new[] { "Inspector" } },
                new { Id = Guid.Parse("11111111-1111-0000-0000-000000000004"), Email = "ana.gonzalez@itopacifico.cl", UserName = "ana.gonzalez", FirstName = "Ana", LastName = "González Muñoz", Rut = "15.678.901-2", Position = "Inspector ITO", TenantId = tenant1Id, Roles = new[] { "Inspector" } },
                new { Id = Guid.Parse("11111111-1111-0000-0000-000000000005"), Email = "juan.herrera@estructurassur.cl", UserName = "juan.herrera", FirstName = "Juan", LastName = "Herrera Lagos", Rut = "16.789.012-3", Position = "Jefe de Obra", TenantId = tenant1Id, Roles = new[] { "Contratista" } },
                new { Id = Guid.Parse("22222222-1111-0000-0000-000000000001"), Email = "admin@consultores-sur.cl", UserName = "admin.sur", FirstName = "Marcela", LastName = "Ríos Pinto", Rut = "17.890.123-4", Position = "Directora", TenantId = tenant2Id, Roles = new[] { "AdminTenant" } },
                new { Id = Guid.Parse("22222222-1111-0000-0000-000000000002"), Email = "diego.silva@consultores-sur.cl", UserName = "diego.silva", FirstName = "Diego", LastName = "Silva Araya", Rut = "18.901.234-5", Position = "Inspector", TenantId = tenant2Id, Roles = new[] { "Inspector" } },
            };

            // 4. Crear o resetear password de cada usuario
            foreach (var u in testUsers)
            {
                var existing = await userManager.FindByEmailAsync(u.Email);
                if (existing != null)
                {
                    // Resetear contraseña para que funcione
                    var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                    var resetResult = await userManager.ResetPasswordAsync(existing, token, password);
                    // Asegurar que está activo
                    existing.IsActive = true;
                    existing.IsDeleted = false;
                    existing.LockoutEnd = null;
                    await userManager.UpdateAsync(existing);
                    // Asegurar roles
                    foreach (var role in u.Roles)
                    {
                        try
                        {
                            if (!await userManager.IsInRoleAsync(existing, role))
                                await userManager.AddToRoleAsync(existing, role);
                        }
                        catch { /* rol ya asignado o duplicado */ }
                    }
                    results.Add(new { u.Email, action = "reset_password", ok = resetResult.Succeeded, errors = resetResult.Errors.Select(e => e.Description) });
                }
                else
                {
                    // Crear usuario nuevo
                    var newUser = new ITO.Cloud.Domain.Entities.Identity.ApplicationUser
                    {
                        Id = u.Id,
                        Email = u.Email, UserName = u.UserName,
                        FirstName = u.FirstName, LastName = u.LastName,
                        Rut = u.Rut, Position = u.Position,
                        TenantId = u.TenantId, IsActive = true,
                        EmailConfirmed = true, CreatedAt = DateTime.UtcNow
                    };
                    var createResult = await userManager.CreateAsync(newUser, password);
                    if (createResult.Succeeded)
                        foreach (var role in u.Roles)
                            await userManager.AddToRoleAsync(newUser, role);
                    results.Add(new { u.Email, action = "created", ok = createResult.Succeeded, errors = createResult.Errors.Select(e => e.Description) });
                }
            }

            return Results.Ok(new { success = true, password, results });
          } catch (Exception ex) { return Results.Json(new { error = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace }, statusCode: 500); }
        });
    }

    // ── Seed de datos completos (solo Development) ────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.MapPost("/dev/seed-data", async (
            ITO.Cloud.Infrastructure.Persistence.ApplicationDbContext db) =>
        {
          try {
            var t1 = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var adminId  = Guid.Parse("11111111-1111-0000-0000-000000000001");
            var carlosId = Guid.Parse("11111111-1111-0000-0000-000000000002");
            var pabloId  = Guid.Parse("11111111-1111-0000-0000-000000000003");
            var anaId    = Guid.Parse("11111111-1111-0000-0000-000000000004");
            var juanId   = Guid.Parse("11111111-1111-0000-0000-000000000005");
            var now = DateTime.UtcNow;
            var created = new List<string>();

            // ── EMPRESAS ────────────────────────────────────────────
            var comp1Id = Guid.Parse("11111111-2222-0000-0000-000000000001");
            var comp2Id = Guid.Parse("11111111-2222-0000-0000-000000000002");
            if (await db.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == comp1Id) == null)
            {
                db.Companies.Add(new ITO.Cloud.Domain.Entities.Projects.Company { Id = comp1Id, TenantId = t1, Name = "Constructora Pacífico SpA", Rut = "96.123.456-7", BusinessName = "Constructora Pacífico Sociedad por Acciones", CompanyType = "constructora", Address = "Av. Apoquindo 4501, Of. 702", City = "Santiago", Region = "Región Metropolitana", Phone = "+56 2 2345 6789", Email = "contacto@constructorapacifico.cl", Website = "www.constructorapacifico.cl", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Companies.Add(new ITO.Cloud.Domain.Entities.Projects.Company { Id = comp2Id, TenantId = t1, Name = "Inmobiliaria Las Palmas S.A.", Rut = "97.234.567-8", BusinessName = "Inmobiliaria Las Palmas Sociedad Anónima", CompanyType = "inmobiliaria", Address = "Av. Las Condes 11.000, Piso 3", City = "Las Condes", Region = "Región Metropolitana", Phone = "+56 2 2456 7890", Email = "info@laspalmasinmobiliaria.cl", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("empresas");
            }

            // ── ESPECIALIDADES ──────────────────────────────────────
            var specEst = Guid.Parse("11111111-6666-0000-0000-000000000001");
            var specTer = Guid.Parse("11111111-6666-0000-0000-000000000002");
            var specIns = Guid.Parse("11111111-6666-0000-0000-000000000003");
            var specImp = Guid.Parse("11111111-6666-0000-0000-000000000004");
            if (await db.Specialties.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == specEst) == null)
            {
                db.Specialties.Add(new ITO.Cloud.Domain.Entities.Projects.Specialty { Id = specEst, TenantId = t1, Name = "Estructural", Code = "EST", Description = "Hormigón, fierros, moldajes y obra gruesa", Color = "#1F3864", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Specialties.Add(new ITO.Cloud.Domain.Entities.Projects.Specialty { Id = specTer, TenantId = t1, Name = "Terminaciones", Code = "TER", Description = "Pisos, revestimientos, pintura y ventanas", Color = "#26A69A", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Specialties.Add(new ITO.Cloud.Domain.Entities.Projects.Specialty { Id = specIns, TenantId = t1, Name = "Instalaciones", Code = "INS", Description = "Instalaciones eléctricas, sanitarias y gas", Color = "#F4A261", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Specialties.Add(new ITO.Cloud.Domain.Entities.Projects.Specialty { Id = specImp, TenantId = t1, Name = "Impermeabilización", Code = "IMP", Description = "Techumbres, terrazas y muros perimetrales", Color = "#E76F51", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("especialidades");
            }

            // ── CONTRATISTAS ────────────────────────────────────────
            var contr1 = Guid.Parse("11111111-7777-0000-0000-000000000001");
            var contr2 = Guid.Parse("11111111-7777-0000-0000-000000000002");
            var contr3 = Guid.Parse("11111111-7777-0000-0000-000000000003");
            if (await db.Set<ITO.Cloud.Domain.Entities.Projects.Contractor>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == contr1) == null)
            {
                db.Set<ITO.Cloud.Domain.Entities.Projects.Contractor>().Add(new() { Id = contr1, TenantId = t1, CompanyId = comp1Id, Name = "Estructuras Del Sur Ltda", Rut = "79.111.222-3", ContactName = "Miguel Fuentes", ContactEmail = "mfuentes@estructurassur.cl", ContactPhone = "+56 9 8111 2233", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Set<ITO.Cloud.Domain.Entities.Projects.Contractor>().Add(new() { Id = contr2, TenantId = t1, CompanyId = comp1Id, Name = "Terminaciones Norte SpA", Rut = "80.222.333-4", ContactName = "Claudia Vega", ContactEmail = "cvega@terminacionesnorte.cl", ContactPhone = "+56 9 8222 3344", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Set<ITO.Cloud.Domain.Entities.Projects.Contractor>().Add(new() { Id = contr3, TenantId = t1, CompanyId = comp1Id, Name = "Instalaciones Rápidas SA", Rut = "81.333.444-5", ContactName = "Andrés Mora", ContactEmail = "amora@instalrapidas.cl", ContactPhone = "+56 9 8333 4455", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("contratistas");
            }
            await db.SaveChangesAsync();

            // ── PROYECTOS ───────────────────────────────────────────
            var proj1 = Guid.Parse("11111111-3333-0000-0000-000000000001");
            var proj2 = Guid.Parse("11111111-3333-0000-0000-000000000002");
            if (await db.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == proj1) == null)
            {
                db.Projects.Add(new ITO.Cloud.Domain.Entities.Projects.Project { Id = proj1, TenantId = t1, CompanyId = comp2Id, Code = "LP-2024-001", Name = "Edificio Las Palmas — Torre Norte", Description = "Edificio residencial de 18 pisos con 120 departamentos, 2 subterráneos y áreas comunes.", ProjectType = "edificio", Status = "activo", Address = "Av. Las Palmas 1250", City = "Vitacura", Region = "Región Metropolitana", Latitude = -33.3890m, Longitude = -70.5765m, StartDate = new DateOnly(2024, 3, 1), EstimatedEndDate = new DateOnly(2026, 9, 30), TotalUnits = 120, ItoManagerId = carlosId, MandanteName = "Inmobiliaria Las Palmas S.A.", MandanteContact = "Pedro Contreras Lara", MandanteEmail = "pcontreras@laspalmasinmobiliaria.cl", ConstructionPermit = "DOM-VIT-2024-0123", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.Projects.Add(new ITO.Cloud.Domain.Entities.Projects.Project { Id = proj2, TenantId = t1, CompanyId = comp1Id, Code = "CRN-2024-002", Name = "Conjunto Residencial Norte — Fase I", Description = "Conjunto de 60 casas pareadas de 2 pisos, urbanización completa.", ProjectType = "conjunto_casas", Status = "activo", Address = "Parcela 5, Camino El Valle Km 12", City = "Colina", Region = "Región Metropolitana", Latitude = -33.2012m, Longitude = -70.6703m, StartDate = new DateOnly(2024, 6, 1), EstimatedEndDate = new DateOnly(2025, 12, 31), TotalUnits = 60, ItoManagerId = carlosId, MandanteName = "Constructora Pacífico SpA", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("proyectos");
            }
            await db.SaveChangesAsync();

            // ── ETAPAS ──────────────────────────────────────────────
            var stage1 = Guid.Parse("11111111-4444-0000-0000-000000000001");
            var stage2 = Guid.Parse("11111111-4444-0000-0000-000000000002");
            var stage3 = Guid.Parse("11111111-4444-0000-0000-000000000003");
            if (await db.ProjectStages.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == stage1) == null)
            {
                db.ProjectStages.Add(new() { Id = stage1, TenantId = t1, ProjectId = proj1, Name = "Obra Gruesa", Description = "Estructura, hormigón y fierros", OrderIndex = 1, Status = "completada", StartDate = new DateOnly(2024, 3, 1), EndDate = new DateOnly(2025, 2, 28), IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.ProjectStages.Add(new() { Id = stage2, TenantId = t1, ProjectId = proj1, Name = "Terminaciones", Description = "Pisos, pintura, ventanas y revestimientos", OrderIndex = 2, Status = "en_progreso", StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2026, 3, 31), IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.ProjectStages.Add(new() { Id = stage3, TenantId = t1, ProjectId = proj1, Name = "Instalaciones y Recepción", Description = "EESS, AACC, entrega final", OrderIndex = 3, Status = "pendiente", StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2026, 9, 30), IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("etapas");
            }

            // ── SECTORES ────────────────────────────────────────────
            var sect1 = Guid.Parse("11111111-5555-0000-0000-000000000001");
            var sect2 = Guid.Parse("11111111-5555-0000-0000-000000000002");
            var sect3 = Guid.Parse("11111111-5555-0000-0000-000000000003");
            if (await db.ProjectSectors.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == sect1) == null)
            {
                db.ProjectSectors.Add(new() { Id = sect1, TenantId = t1, ProjectId = proj1, Name = "Torre A", SectorType = "torre", OrderIndex = 1, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.ProjectSectors.Add(new() { Id = sect2, TenantId = t1, ProjectId = proj1, Name = "Torre B", SectorType = "torre", OrderIndex = 2, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.ProjectSectors.Add(new() { Id = sect3, TenantId = t1, ProjectId = proj1, Name = "Subterráneo -1", SectorType = "subterraneo", OrderIndex = 3, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("sectores");
            }

            // ── UNIDADES ────────────────────────────────────────────
            var unit1 = Guid.Parse("11111111-ffff-0000-0000-000000000001");
            if (await db.ProjectUnits.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == unit1) == null)
            {
                for (int i = 1; i <= 6; i++)
                {
                    var sectorId = i <= 3 ? sect1 : sect2;
                    var floor = i <= 3 ? 3 : 4;
                    var code = i <= 3 ? $"30{i}" : $"40{i - 3}";
                    db.ProjectUnits.Add(new() { Id = Guid.Parse($"11111111-ffff-0000-0000-00000000000{i}"), TenantId = t1, ProjectId = proj1, SectorId = sectorId, UnitCode = code, UnitType = "departamento", Floor = floor, SurfaceM2 = 67.50m, Status = "recepcion_preliminar", IsActive = true, CreatedAt = now, CreatedBy = adminId });
                }
                created.Add("unidades");
            }

            // ── MIEMBROS DEL PROYECTO ───────────────────────────────
            if (!await db.ProjectMembers.IgnoreQueryFilters().AnyAsync(m => m.ProjectId == proj1))
            {
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, UserId = carlosId, ProjectRole = "supervisor", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, UserId = pabloId, ProjectRole = "inspector", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, UserId = anaId, ProjectRole = "inspector", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, UserId = juanId, ProjectRole = "contratista", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj2, UserId = carlosId, ProjectRole = "supervisor", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                db.ProjectMembers.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj2, UserId = pabloId, ProjectRole = "inspector", IsActive = true, AssignedAt = now, AssignedBy = adminId });
                created.Add("miembros");
            }
            await db.SaveChangesAsync();

            // ── PLANTILLAS DE INSPECCIÓN ────────────────────────────
            var tmpl1 = Guid.Parse("11111111-8888-0000-0000-000000000001");
            var tmpl2 = Guid.Parse("11111111-8888-0000-0000-000000000002");
            var tmpl3 = Guid.Parse("11111111-8888-0000-0000-000000000003");
            if (await db.InspectionTemplates.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tmpl1) == null)
            {
                db.InspectionTemplates.Add(new() { Id = tmpl1, TenantId = t1, Name = "Inspección Obra Gruesa v2", Description = "Checklist completo para revisión de hormigón armado, enfierradura y moldajes.", TemplateType = "obra_gruesa", SpecialtyId = specEst, Status = ITO.Cloud.Domain.Enums.TemplateStatus.Activa, CurrentVersion = 2, AllowPartialSave = true, RequireGeolocation = true, PassingScore = 70.0m, CreatedAt = now, CreatedBy = adminId });
                db.InspectionTemplates.Add(new() { Id = tmpl2, TenantId = t1, Name = "Inspección Terminaciones", Description = "Revisión de pisos, revestimientos, pintura, ventanas y puertas.", TemplateType = "terminaciones", SpecialtyId = specTer, Status = ITO.Cloud.Domain.Enums.TemplateStatus.Activa, CurrentVersion = 1, AllowPartialSave = true, PassingScore = 80.0m, CreatedAt = now, CreatedBy = adminId });
                db.InspectionTemplates.Add(new() { Id = tmpl3, TenantId = t1, Name = "Recepción de Unidad", Description = "Formulario de recepción de departamento o casa al propietario.", TemplateType = "recepcion", Status = ITO.Cloud.Domain.Enums.TemplateStatus.Activa, CurrentVersion = 1, IsGlobal = true, AllowPartialSave = true, PassingScore = 90.0m, CreatedAt = now, CreatedBy = adminId });
                created.Add("plantillas");
            }

            // ── SECCIONES DE PLANTILLA ──────────────────────────────
            var sec1 = Guid.Parse("11111111-9999-0000-0000-000000000001");
            var sec2 = Guid.Parse("11111111-9999-0000-0000-000000000002");
            var sec3 = Guid.Parse("11111111-9999-0000-0000-000000000003");
            var sec4 = Guid.Parse("11111111-9999-0000-0000-000000000010");
            var sec5 = Guid.Parse("11111111-9999-0000-0000-000000000011");
            if (await db.TemplateSections.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == sec1) == null)
            {
                db.TemplateSections.Add(new() { Id = sec1, TenantId = t1, TemplateId = tmpl1, Title = "Hormigón", Description = "Revisión de calidad del vaciado y curado", OrderIndex = 1, IsRequired = true, Weight = 1.5m, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.TemplateSections.Add(new() { Id = sec2, TenantId = t1, TemplateId = tmpl1, Title = "Enfierradura", Description = "Control de enfierradura antes del vaciado", OrderIndex = 2, IsRequired = true, Weight = 2.0m, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.TemplateSections.Add(new() { Id = sec3, TenantId = t1, TemplateId = tmpl1, Title = "Moldajes", Description = "Revisión de moldajes y puntales", OrderIndex = 3, IsRequired = true, Weight = 1.0m, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.TemplateSections.Add(new() { Id = sec4, TenantId = t1, TemplateId = tmpl2, Title = "Pisos", Description = "Control de instalación y terminación de pisos", OrderIndex = 1, IsRequired = true, Weight = 1.0m, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                db.TemplateSections.Add(new() { Id = sec5, TenantId = t1, TemplateId = tmpl2, Title = "Muros y Pintura", Description = "Revisión de estucos, revestimientos y pintura", OrderIndex = 2, IsRequired = true, Weight = 1.0m, IsActive = true, CreatedAt = now, CreatedBy = adminId });
                created.Add("secciones");
            }

            // ── PREGUNTAS ───────────────────────────────────────────
            var q1 = Guid.Parse("11111111-aaaa-0000-0000-000000000001");
            if (await db.TemplateQuestions.IgnoreQueryFilters().FirstOrDefaultAsync(q => q.Id == q1) == null)
            {
                db.TemplateQuestions.Add(new() { Id = q1, TenantId = t1, SectionId = sec1, QuestionText = "¿El hormigón presenta resistencia certificada f'c ≥ 250 kg/cm²?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 1, IsRequired = true, IsCritical = true, Weight = 2.0m, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000002"), TenantId = t1, SectionId = sec1, QuestionText = "¿El proceso de curado cumple el tiempo mínimo requerido (7 días)?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 2, IsRequired = true, IsCritical = true, Weight = 1.5m, MinPhotos = 1, MaxPhotos = 3, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000003"), TenantId = t1, SectionId = sec1, QuestionText = "Temperatura ambiente al momento del vaciado (°C)", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.Numeric, OrderIndex = 3, IsRequired = true, Weight = 1.0m, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000004"), TenantId = t1, SectionId = sec2, QuestionText = "¿El diámetro de barras corresponde al plano estructural?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 1, IsRequired = true, IsCritical = true, Weight = 2.0m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000005"), TenantId = t1, SectionId = sec2, QuestionText = "¿Los empalmes cumplen la longitud mínima de traslapo?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 2, IsRequired = true, IsCritical = true, Weight = 2.0m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000006"), TenantId = t1, SectionId = sec2, QuestionText = "¿Los recubrimientos son los especificados (mín. 2 cm)?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 3, IsRequired = true, Weight = 1.5m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000007"), TenantId = t1, SectionId = sec3, QuestionText = "Estado general del moldaje", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.MultipleChoice, OrderIndex = 1, IsRequired = true, Weight = 1.0m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000008"), TenantId = t1, SectionId = sec4, QuestionText = "¿La nivelación del piso cumple tolerancia ±3mm/2m?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 1, IsRequired = true, Weight = 1.0m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                db.TemplateQuestions.Add(new() { Id = Guid.Parse("11111111-aaaa-0000-0000-000000000009"), TenantId = t1, SectionId = sec5, QuestionText = "¿La pintura cubre uniformemente sin escurrimientos ni burbujas?", QuestionType = ITO.Cloud.Domain.Enums.QuestionType.YesNo, OrderIndex = 1, IsRequired = true, Weight = 1.0m, MinPhotos = 1, CreatedAt = now, CreatedBy = adminId });
                created.Add("preguntas");
            }

            // ── OPCIONES DE PREGUNTA (moldaje) ──────────────────────
            var opt1 = Guid.Parse("11111111-bbbb-0000-0000-000000000001");
            if (await db.TemplateQuestionOptions.FirstOrDefaultAsync(o => o.Id == opt1) == null)
            {
                db.TemplateQuestionOptions.Add(new() { Id = opt1, QuestionId = Guid.Parse("11111111-aaaa-0000-0000-000000000007"), Label = "Bueno", Value = "bueno", OrderIndex = 1, Score = 1.0m });
                db.TemplateQuestionOptions.Add(new() { Id = Guid.Parse("11111111-bbbb-0000-0000-000000000002"), QuestionId = Guid.Parse("11111111-aaaa-0000-0000-000000000007"), Label = "Regular", Value = "regular", OrderIndex = 2, Score = 0.5m });
                db.TemplateQuestionOptions.Add(new() { Id = Guid.Parse("11111111-bbbb-0000-0000-000000000003"), QuestionId = Guid.Parse("11111111-aaaa-0000-0000-000000000007"), Label = "Deficiente", Value = "deficiente", OrderIndex = 3, IsFailureOption = true, Score = 0.0m });
                created.Add("opciones");
            }
            await db.SaveChangesAsync();

            // ── INSPECCIONES ────────────────────────────────────────
            var ins1 = Guid.Parse("11111111-cccc-0000-0000-000000000001");
            if (await db.Inspections.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == ins1) == null)
            {
                db.Inspections.Add(new() { Id = ins1, TenantId = t1, ProjectId = proj1, TemplateId = tmpl1, TemplateVersion = 2, StageId = stage1, SectorId = sect1, Code = "INS-2025-001", Title = "Inspección Obra Gruesa — Torre A Piso 8", Description = "Control de vaciado de losa nivel 8 Torre A.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Cerrada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2025-06-15 09:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2025-06-15 09:30:00Z").ToUniversalTime(), FinishedAt = DateTime.Parse("2025-06-15 11:45:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr1, SpecialtyId = specEst, Score = 85.5m, PassingScore = 70.0m, Passed = true, TotalQuestions = 7, AnsweredQuestions = 7, ConformingCount = 6, NonConformingCount = 1, Latitude = -33.3890m, Longitude = -70.5765m, WeatherConditions = "Soleado", Temperature = 22.5m, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.Parse("11111111-cccc-0000-0000-000000000002"), TenantId = t1, ProjectId = proj1, TemplateId = tmpl2, TemplateVersion = 1, StageId = stage2, SectorId = sect1, Code = "INS-2025-002", Title = "Inspección Terminaciones — Torre A Pisos 5-6", Description = "Revisión de pisos y muros terminados en pisos 5 y 6.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.EnProceso, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Alta, ScheduledDate = DateTime.Parse("2026-03-18 10:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2026-03-18 10:15:00Z").ToUniversalTime(), AssignedToId = anaId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr2, SpecialtyId = specTer, PassingScore = 80.0m, TotalQuestions = 4, AnsweredQuestions = 2, ConformingCount = 1, NonConformingCount = 1, Latitude = -33.3891m, Longitude = -70.5766m, WeatherConditions = "Nublado", Temperature = 18.0m, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.Parse("11111111-cccc-0000-0000-000000000003"), TenantId = t1, ProjectId = proj1, TemplateId = tmpl1, TemplateVersion = 2, StageId = stage2, SectorId = sect2, Code = "INS-2025-003", Title = "Inspección Obra Gruesa — Torre B Piso 10", Description = "Control de enfierradura y moldajes antes del vaciado.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Programada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2026-03-25 09:00:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr1, SpecialtyId = specEst, PassingScore = 70.0m, TotalQuestions = 7, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.Parse("11111111-cccc-0000-0000-000000000004"), TenantId = t1, ProjectId = proj1, TemplateId = tmpl2, TemplateVersion = 1, StageId = stage2, SectorId = sect1, Code = "INS-2025-004", Title = "Inspección Terminaciones — Torre A Pisos 3-4", Description = "Detectadas observaciones en revestimientos.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Observada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Alta, ScheduledDate = DateTime.Parse("2026-03-10 09:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2026-03-10 09:20:00Z").ToUniversalTime(), FinishedAt = DateTime.Parse("2026-03-10 12:00:00Z").ToUniversalTime(), AssignedToId = anaId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr2, SpecialtyId = specTer, Score = 58.0m, PassingScore = 80.0m, Passed = false, TotalQuestions = 4, AnsweredQuestions = 4, ConformingCount = 2, NonConformingCount = 2, Latitude = -33.3892m, Longitude = -70.5767m, WeatherConditions = "Soleado", Temperature = 20.0m, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.Parse("11111111-cccc-0000-0000-000000000005"), TenantId = t1, ProjectId = proj2, TemplateId = tmpl1, TemplateVersion = 2, Code = "INS-2025-005", Title = "Inspección Obra Gruesa — Casas Sector Norte Bloque 1", Description = "Control de losas de casas pareadas bloque 1-10.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Finalizada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2025-11-20 09:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2025-11-20 09:10:00Z").ToUniversalTime(), FinishedAt = DateTime.Parse("2025-11-20 13:00:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr1, SpecialtyId = specEst, Score = 92.0m, PassingScore = 70.0m, Passed = true, TotalQuestions = 7, AnsweredQuestions = 7, ConformingCount = 7, Latitude = -33.2012m, Longitude = -70.6703m, WeatherConditions = "Soleado", Temperature = 25.0m, CreatedAt = now, CreatedBy = adminId });

                // Inspecciones adicionales para más variedad
                db.Inspections.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, TemplateId = tmpl3, TemplateVersion = 1, StageId = stage2, SectorId = sect1, Code = "INS-2026-006", Title = "Recepción Preliminar — Depto 301", Description = "Pre-recepción departamento 301 Torre A, piso 3.", InspectionType = "recepcion_preliminar", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Programada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2026-04-01 09:00:00Z").ToUniversalTime(), AssignedToId = anaId, AssignedById = carlosId, SupervisorId = carlosId, SpecialtyId = specTer, PassingScore = 90.0m, TotalQuestions = 9, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, TemplateId = tmpl1, TemplateVersion = 2, StageId = stage2, SectorId = sect2, Code = "INS-2026-007", Title = "Inspección Obra Gruesa — Torre B Piso 12", Description = "Control de vaciado de losa nivel 12 Torre B.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Programada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Alta, ScheduledDate = DateTime.Parse("2026-04-05 08:30:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr1, SpecialtyId = specEst, PassingScore = 70.0m, TotalQuestions = 7, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, TemplateId = tmpl2, TemplateVersion = 1, StageId = stage2, SectorId = sect1, Code = "INS-2026-008", Title = "Inspección Terminaciones — Torre A Pisos 7-8", Description = "Revisión terminaciones pisos 7 y 8.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Finalizada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2026-03-05 10:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2026-03-05 10:10:00Z").ToUniversalTime(), FinishedAt = DateTime.Parse("2026-03-05 13:30:00Z").ToUniversalTime(), AssignedToId = anaId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr2, SpecialtyId = specTer, Score = 88.0m, PassingScore = 80.0m, Passed = true, TotalQuestions = 4, AnsweredQuestions = 4, ConformingCount = 3, NonConformingCount = 1, Latitude = -33.3890m, Longitude = -70.5765m, WeatherConditions = "Parcialmente nublado", Temperature = 21.0m, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj2, TemplateId = tmpl1, TemplateVersion = 2, Code = "INS-2026-009", Title = "Inspección Obra Gruesa — Casas Sector Sur Bloque 2", Description = "Control de cimientos y radieres casas 11-20.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Cerrada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Normal, ScheduledDate = DateTime.Parse("2026-01-15 09:00:00Z").ToUniversalTime(), StartedAt = DateTime.Parse("2026-01-15 09:15:00Z").ToUniversalTime(), FinishedAt = DateTime.Parse("2026-01-15 12:00:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, ContractorId = contr1, SpecialtyId = specEst, Score = 95.0m, PassingScore = 70.0m, Passed = true, TotalQuestions = 7, AnsweredQuestions = 7, ConformingCount = 7, Latitude = -33.2012m, Longitude = -70.6703m, WeatherConditions = "Despejado", Temperature = 28.0m, CreatedAt = now, CreatedBy = adminId });

                db.Inspections.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, TemplateId = tmpl1, TemplateVersion = 2, StageId = stage2, SectorId = sect1, Code = "INS-2026-010", Title = "Inspección Obra Gruesa — Torre A Piso 15", Description = "Control de vaciado de losa nivel 15.", InspectionType = "ordinaria", Status = ITO.Cloud.Domain.Enums.InspectionStatus.Cancelada, Priority = ITO.Cloud.Domain.Enums.InspectionPriority.Baja, ScheduledDate = DateTime.Parse("2026-02-20 09:00:00Z").ToUniversalTime(), AssignedToId = pabloId, AssignedById = carlosId, SupervisorId = carlosId, Notes = "Cancelada por condiciones climáticas adversas (lluvia).", CreatedAt = now, CreatedBy = adminId });

                created.Add("inspecciones (10)");
            }
            await db.SaveChangesAsync();

            // ── OBSERVACIONES ───────────────────────────────────────
            var obs1 = Guid.Parse("11111111-dddd-0000-0000-000000000001");
            if (await db.Observations.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == obs1) == null)
            {
                db.Observations.Add(new() { Id = obs1, TenantId = t1, ProjectId = proj1, InspectionId = ins1, Code = "OBS-2025-001", Title = "Recubrimiento insuficiente en enfierradura — Torre A P8", Description = "Recubrimiento medido de 1.5 cm, norma exige mínimo 2.0 cm en losa de piso. Se evidencia en 3 puntos distintos del sector norponiente.", SpecialtyId = specEst, Category = "hormigon_armado", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Alta, Status = ITO.Cloud.Domain.Enums.ObservationStatus.EnCorreccion, StageId = stage1, SectorId = sect1, LocationDescription = "Losa piso 8, sector norponiente entre ejes C-D / 3-4", ContractorId = contr1, AssignedToId = juanId, AssignedById = carlosId, DetectedAt = DateTime.Parse("2025-06-15 12:00:00Z").ToUniversalTime(), DetectedBy = pabloId, DueDate = new DateOnly(2025, 6, 30), RootCause = "Error de instalación de separadores de enfierradura.", CorrectiveAction = "Demolición y reconstrucción del sector afectado con separadores certificados.", Latitude = -33.3890m, Longitude = -70.5765m, CreatedAt = now, CreatedBy = adminId });

                db.Observations.Add(new() { Id = Guid.Parse("11111111-dddd-0000-0000-000000000002"), TenantId = t1, ProjectId = proj1, InspectionId = Guid.Parse("11111111-cccc-0000-0000-000000000004"), Code = "OBS-2026-001", Title = "Nivelación de piso fuera de tolerancia — Torre A P3", Description = "Medición de planimetría detecta desviación de 6mm en 2m lineal. Tolerancia máxima según especificación técnica es ±3mm.", SpecialtyId = specTer, Category = "terminaciones", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Media, Status = ITO.Cloud.Domain.Enums.ObservationStatus.Asignada, StageId = stage2, SectorId = sect1, LocationDescription = "Departamentos 301, 302 y 303 — sala comedor", ContractorId = contr2, AssignedToId = juanId, AssignedById = carlosId, DetectedAt = DateTime.Parse("2026-03-10 12:00:00Z").ToUniversalTime(), DetectedBy = anaId, DueDate = new DateOnly(2026, 4, 15), RootCause = "Deficiencia en control de nivelación durante instalación de contrapiso.", CorrectiveAction = "Escarificado y nivelación con mortero autonivelante en zonas afectadas.", Latitude = -33.3891m, Longitude = -70.5767m, CreatedAt = now, CreatedBy = adminId });

                db.Observations.Add(new() { Id = Guid.Parse("11111111-dddd-0000-0000-000000000003"), TenantId = t1, ProjectId = proj1, InspectionId = Guid.Parse("11111111-cccc-0000-0000-000000000004"), Code = "OBS-2026-002", Title = "Manchas de humedad en muro sur — Torre A P4", Description = "Se observan manchas de humedad activa en muro perimetral sur, posible filtración por junta de expansión.", SpecialtyId = specImp, Category = "impermeabilizacion", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Alta, Status = ITO.Cloud.Domain.Enums.ObservationStatus.Abierta, StageId = stage2, SectorId = sect1, LocationDescription = "Muro sur departamento 401, altura 1.2m desde el piso", ContractorId = contr2, AssignedById = carlosId, DetectedAt = DateTime.Parse("2026-03-10 12:30:00Z").ToUniversalTime(), DetectedBy = anaId, DueDate = new DateOnly(2026, 4, 10), Latitude = -33.3892m, Longitude = -70.5766m, CreatedAt = now, CreatedBy = adminId });

                db.Observations.Add(new() { Id = Guid.Parse("11111111-dddd-0000-0000-000000000004"), TenantId = t1, ProjectId = proj1, InspectionId = ins1, Code = "OBS-2025-002", Title = "Fisura en viga secundaria — Torre A P8", Description = "Fisura de 0.3mm en viga secundaria en zona de apoyo. Requiere evaluación de ingeniero calculista.", SpecialtyId = specEst, Category = "estructura", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Critica, Status = ITO.Cloud.Domain.Enums.ObservationStatus.Cerrada, StageId = stage1, SectorId = sect1, LocationDescription = "Viga VS-15 eje D entre ejes 3-4 piso 8", ContractorId = contr1, AssignedToId = juanId, AssignedById = carlosId, DetectedAt = DateTime.Parse("2025-06-15 13:00:00Z").ToUniversalTime(), DetectedBy = pabloId, DueDate = new DateOnly(2025, 7, 15), RootCause = "Deformación diferencial durante curado por temperatura excesiva.", CorrectiveAction = "Inyección de resina epóxica certificada + informe de ingeniero calculista aprobado.", ClosedAt = DateTime.Parse("2025-07-10 15:00:00Z").ToUniversalTime(), ClosedBy = carlosId, Latitude = -33.3890m, Longitude = -70.5765m, CreatedAt = now, CreatedBy = adminId });

                db.Observations.Add(new() { Id = Guid.Parse("11111111-dddd-0000-0000-000000000005"), TenantId = t1, ProjectId = proj2, InspectionId = Guid.Parse("11111111-cccc-0000-0000-000000000005"), Code = "OBS-2025-003", Title = "Traslapo insuficiente en enfierradura horizontal — Casa 5", Description = "Traslapo medido de 28 cm, especificación exige mínimo 35 cm.", SpecialtyId = specEst, Category = "hormigon_armado", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Alta, Status = ITO.Cloud.Domain.Enums.ObservationStatus.Cerrada, LocationDescription = "Losa de techo casa 5 sector norte, eje perimetral", ContractorId = contr1, AssignedToId = juanId, AssignedById = carlosId, DetectedAt = DateTime.Parse("2025-11-20 13:30:00Z").ToUniversalTime(), DetectedBy = pabloId, DueDate = new DateOnly(2025, 12, 5), RootCause = "Corte de barra en posición incorrecta por falta de control de planos.", CorrectiveAction = "Adición de longitud de traslapo mediante soldadura certificada + control radiográfico.", ClosedAt = DateTime.Parse("2025-12-03 10:00:00Z").ToUniversalTime(), ClosedBy = carlosId, Latitude = -33.2012m, Longitude = -70.6703m, CreatedAt = now, CreatedBy = adminId });

                // Observaciones adicionales
                db.Observations.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, Code = "OBS-2026-003", Title = "Puerta de acceso descuadrada — Depto 302", Description = "La puerta principal del departamento 302 no cierra correctamente, marco descuadrado 4mm.", SpecialtyId = specTer, Category = "terminaciones", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Baja, Status = ITO.Cloud.Domain.Enums.ObservationStatus.Abierta, StageId = stage2, SectorId = sect1, LocationDescription = "Acceso principal depto 302, piso 3 Torre A", ContractorId = contr2, DetectedAt = now.AddDays(-5), DetectedBy = anaId, DueDate = DateOnly.FromDateTime(now.AddDays(20)), CreatedAt = now, CreatedBy = adminId });

                db.Observations.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ProjectId = proj1, Code = "OBS-2026-004", Title = "Fuga en válvula de gas — Depto 401", Description = "Se detecta micro fuga en unión de válvula de corte de gas cocina.", SpecialtyId = specIns, Category = "instalaciones", Severity = ITO.Cloud.Domain.Enums.ObservationSeverity.Critica, Status = ITO.Cloud.Domain.Enums.ObservationStatus.EnCorreccion, StageId = stage2, SectorId = sect2, LocationDescription = "Cocina depto 401, Torre B — válvula de corte sobre encimera", ContractorId = contr3, AssignedToId = juanId, AssignedById = carlosId, DetectedAt = now.AddDays(-2), DetectedBy = pabloId, DueDate = DateOnly.FromDateTime(now.AddDays(3)), RootCause = "Sello de teflón insuficiente en unión roscada.", CreatedAt = now, CreatedBy = adminId });

                created.Add("observaciones (7)");
            }
            await db.SaveChangesAsync();

            // ── HISTORIAL DE OBSERVACIONES ──────────────────────────
            if (!await db.ObservationHistory.IgnoreQueryFilters().AnyAsync(h => h.ObservationId == obs1))
            {
                db.ObservationHistory.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ObservationId = obs1, Action = "creada", NewStatus = "abierta", Comment = "Observación registrada durante inspección INS-2025-001.", CreatedAt = DateTime.Parse("2025-06-15 12:00:00Z").ToUniversalTime(), CreatedBy = pabloId });
                db.ObservationHistory.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ObservationId = obs1, Action = "asignada", PreviousStatus = "abierta", NewStatus = "asignada", NewAssignedTo = juanId, Comment = "Asignada a contratista Estructuras Del Sur para corrección.", CreatedAt = DateTime.Parse("2025-06-16 08:00:00Z").ToUniversalTime(), CreatedBy = carlosId });
                db.ObservationHistory.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ObservationId = obs1, Action = "estado_cambiado", PreviousStatus = "asignada", NewStatus = "en_correccion", Comment = "Contratista inicia trabajos de corrección.", CreatedAt = DateTime.Parse("2025-06-20 08:00:00Z").ToUniversalTime(), CreatedBy = juanId });
                db.ObservationHistory.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ObservationId = Guid.Parse("11111111-dddd-0000-0000-000000000004"), Action = "creada", NewStatus = "abierta", Comment = "Fisura crítica detectada durante inspección.", CreatedAt = DateTime.Parse("2025-06-15 13:00:00Z").ToUniversalTime(), CreatedBy = pabloId });
                db.ObservationHistory.Add(new() { Id = Guid.NewGuid(), TenantId = t1, ObservationId = Guid.Parse("11111111-dddd-0000-0000-000000000004"), Action = "cerrada", PreviousStatus = "corregida", NewStatus = "cerrada", Comment = "Inyección epóxica aprobada por ingeniero calculista. Informe adjunto.", CreatedAt = DateTime.Parse("2025-07-10 15:00:00Z").ToUniversalTime(), CreatedBy = carlosId });
                created.Add("historial");
            }
            await db.SaveChangesAsync();

            // ── SEQUENCE COUNTERS (para códigos auto-generados) ─────
            if (!await db.SequenceCounters.AnyAsync(s => s.TenantId == t1))
            {
                db.SequenceCounters.Add(new() { Id = Guid.NewGuid(), TenantId = t1, EntityType = "inspection", Prefix = "INS", Year = 2026, LastValue = 10 });
                db.SequenceCounters.Add(new() { Id = Guid.NewGuid(), TenantId = t1, EntityType = "observation", Prefix = "OBS", Year = 2026, LastValue = 4 });
                created.Add("contadores");
            }
            await db.SaveChangesAsync();

            return Results.Ok(new { success = true, message = "Datos de prueba insertados.", created });
          } catch (Exception ex) { return Results.Json(new { error = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace }, statusCode: 500); }
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
