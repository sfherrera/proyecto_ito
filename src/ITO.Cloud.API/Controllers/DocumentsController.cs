using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Domain.Entities.Documents;
using ITO.Cloud.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de documentación técnica de proyectos (planos, especificaciones, contratos, etc.)</summary>
public class DocumentsController : BaseApiController
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IWebHostEnvironment env)
    {
        _db          = db;
        _currentUser = currentUser;
        _env         = env;
    }

    /// <summary>Lista todos los documentos, opcionalmente filtrados por proyecto y/o categoría</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.ProjectDocuments
            .AsNoTracking()
            .Include(d => d.Project)
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(d => d.ProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search) || (d.Description != null && d.Description.Contains(search)));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new ProjectDocumentDto(
                d.Id, d.ProjectId, d.Project!.Name, d.Category, d.Name, d.Description,
                d.FileName, d.FileSizeBytes, d.MimeType, d.Version, d.IsActive, d.CreatedAt))
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = items,
            pagination = new { Page = page, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    /// <summary>Obtiene un documento por ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var d = await _db.ProjectDocuments
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d == null)
            return NotFound(new { success = false, message = "Documento no encontrado." });

        return OkData(new ProjectDocumentDto(
            d.Id, d.ProjectId, d.Project?.Name, d.Category, d.Name, d.Description,
            d.FileName, d.FileSizeBytes, d.MimeType, d.Version, d.IsActive, d.CreatedAt));
    }

    /// <summary>Sube un documento técnico asociado a un proyecto</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Operations)]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(
        [FromForm] Guid projectId,
        [FromForm] string name,
        [FromForm] string category,
        [FromForm] string? description,
        [FromForm] string? version,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "Debe adjuntar un archivo." });

        // Verificar que el proyecto existe
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId);
        if (!projectExists)
            return NotFound(new { success = false, message = "Proyecto no encontrado." });

        // Guardar archivo en disco
        var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Documents", projectId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var doc = new ProjectDocument
        {
            TenantId      = _currentUser.TenantId,
            ProjectId     = projectId,
            Category      = category,
            Name          = name,
            Description   = description,
            FileName      = file.FileName,
            FilePath      = filePath,
            FileSizeBytes = file.Length,
            MimeType      = file.ContentType,
            Version       = version,
            IsActive      = true,
            CreatedBy     = _currentUser.UserId
        };

        _db.ProjectDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return CreatedData(new ProjectDocumentDto(
            doc.Id, doc.ProjectId, null, doc.Category, doc.Name, doc.Description,
            doc.FileName, doc.FileSizeBytes, doc.MimeType, doc.Version, doc.IsActive, doc.CreatedAt));
    }

    /// <summary>Actualiza metadatos de un documento (nombre, categoría, descripción, versión)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        var doc = await _db.ProjectDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null)
            return NotFound(new { success = false, message = "Documento no encontrado." });

        doc.Name        = dto.Name;
        doc.Category    = dto.Category;
        doc.Description = dto.Description;
        doc.Version     = dto.Version;
        doc.UpdatedBy   = _currentUser.UserId;
        doc.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return OkData(new ProjectDocumentDto(
            doc.Id, doc.ProjectId, null, doc.Category, doc.Name, doc.Description,
            doc.FileName, doc.FileSizeBytes, doc.MimeType, doc.Version, doc.IsActive, doc.CreatedAt));
    }

    /// <summary>Elimina (soft delete) un documento</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var doc = await _db.ProjectDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null)
            return NotFound(new { success = false, message = "Documento no encontrado." });

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedBy = _currentUser.UserId;

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Documento eliminado." });
    }

    /// <summary>Descarga el archivo del documento</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var doc = await _db.ProjectDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null)
            return NotFound(new { success = false, message = "Documento no encontrado." });

        if (!System.IO.File.Exists(doc.FilePath))
            return NotFound(new { success = false, message = "Archivo no encontrado en el servidor." });

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
        return File(bytes, doc.MimeType ?? "application/octet-stream", doc.FileName);
    }

    /// <summary>Lista las categorías disponibles</summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        var categories = new[]
        {
            new { Value = "plano",           Label = "Plano" },
            new { Value = "especificacion",  Label = "Especificación Técnica" },
            new { Value = "contrato",        Label = "Contrato" },
            new { Value = "procedimiento",   Label = "Procedimiento" },
            new { Value = "permiso",         Label = "Permiso / Autorización" },
            new { Value = "informe",         Label = "Informe" },
            new { Value = "acta",            Label = "Acta" },
            new { Value = "presupuesto",     Label = "Presupuesto" },
            new { Value = "cronograma",      Label = "Cronograma" },
            new { Value = "general",         Label = "General" }
        };
        return OkData(categories);
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record ProjectDocumentDto(
    Guid Id, Guid ProjectId, string? ProjectName,
    string Category, string Name, string? Description,
    string FileName, long? FileSizeBytes, string? MimeType,
    string? Version, bool IsActive, DateTime CreatedAt);

public record UpdateDocumentDto(
    string Name, string Category, string? Description, string? Version);
