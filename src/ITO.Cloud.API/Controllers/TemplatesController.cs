using ITO.Cloud.Domain.Entities.Templates;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de plantillas de inspección</summary>
public class TemplatesController : BaseApiController
{
    private readonly ApplicationDbContext _db;

    public TemplatesController(ApplicationDbContext db) => _db = db;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private Guid GetTenantId() =>
        Guid.Parse(User.FindFirst("tenant_id")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var q = _db.InspectionTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(t => t.Name.Contains(search));

        var templates = await q
            .OrderBy(t => t.Name)
            .Take(200)
            .Select(t => new { t.Id, t.Name, t.TemplateType, Status = t.Status.ToString() })
            .ToListAsync();

        return OkData(templates);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _db.InspectionTemplates.AsNoTracking()
            .Include(t => t.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Questions.OrderBy(q => q.OrderIndex))
                    .ThenInclude(q => q.Options.OrderBy(o => o.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
            return NotFound(new { success = false, message = "Plantilla no encontrada." });

        return OkData(new
        {
            template.Id,
            template.Name,
            template.Description,
            template.TemplateType,
            Status = template.Status.ToString(),
            template.CurrentVersion,
            template.IsGlobal,
            template.AllowPartialSave,
            template.RequireGeolocation,
            template.RequireSignature,
            template.PassingScore,
            template.CreatedAt,
            Sections = template.Sections.Select(s => new
            {
                s.Id,
                s.Title,
                s.Description,
                s.OrderIndex,
                s.IsRequired,
                s.Weight,
                Questions = s.Questions.Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Description,
                    QuestionType = q.QuestionType.ToString(),
                    q.OrderIndex,
                    q.IsRequired,
                    q.IsCritical,
                    q.Weight,
                    q.MinPhotos,
                    q.MaxPhotos,
                    Options = q.Options.Select(o => new
                    {
                        o.Id,
                        o.Label,
                        o.Value,
                        o.IsFailureOption,
                        o.Score
                    })
                })
            })
        });
    }

    // ── POST / — Crear plantilla ─────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest req)
    {
        var userId   = GetUserId();
        var tenantId = GetTenantId();

        var template = new InspectionTemplate
        {
            Name               = req.Name,
            Description        = req.Description,
            TemplateType       = req.TemplateType,
            PassingScore       = req.PassingScore,
            RequireGeolocation = req.RequireGeolocation,
            RequireSignature   = req.RequireSignature,
            Status             = TemplateStatus.Borrador,
            TenantId           = tenantId,
            CreatedBy          = userId
        };

        _db.InspectionTemplates.Add(template);
        await _db.SaveChangesAsync();

        return CreatedData(new { template.Id }, $"/api/templates/{template.Id}");
    }

    // ── PUT {id} — Actualizar plantilla ──────────────────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateRequest req)
    {
        var template = await _db.InspectionTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null)
            return NotFound(new { success = false, message = "Plantilla no encontrada." });

        template.Name               = req.Name ?? template.Name;
        template.Description        = req.Description ?? template.Description;
        template.TemplateType       = req.TemplateType ?? template.TemplateType;
        template.PassingScore       = req.PassingScore ?? template.PassingScore;
        template.RequireGeolocation = req.RequireGeolocation ?? template.RequireGeolocation;
        template.RequireSignature   = req.RequireSignature ?? template.RequireSignature;
        template.UpdatedAt          = DateTime.UtcNow;
        template.UpdatedBy          = GetUserId();

        await _db.SaveChangesAsync();

        return OkData(new { template.Id });
    }

    // ── DELETE {id} — Soft delete ────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var template = await _db.InspectionTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template == null)
            return NotFound(new { success = false, message = "Plantilla no encontrada." });

        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        template.DeletedBy = GetUserId();
        template.Status    = TemplateStatus.Archivada;

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Plantilla eliminada." });
    }

    // ── POST {id}/sections — Agregar sección ─────────────────────────────────
    [HttpPost("{id:guid}/sections")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddSection(Guid id, [FromBody] AddSectionRequest req)
    {
        var template = await _db.InspectionTemplates
            .Include(t => t.Sections)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
            return NotFound(new { success = false, message = "Plantilla no encontrada." });

        var section = new TemplateSection
        {
            TemplateId  = id,
            Title       = req.Title,
            Description = req.Description,
            OrderIndex  = req.OrderIndex ?? (template.Sections.Count + 1),
            Weight      = req.Weight ?? 1.0m,
            IsRequired  = req.IsRequired ?? true,
            TenantId    = GetTenantId(),
            CreatedBy   = GetUserId()
        };

        _db.TemplateSections.Add(section);
        await _db.SaveChangesAsync();

        return CreatedData(new { section.Id, section.Title, section.OrderIndex },
            $"/api/templates/{id}");
    }

    // ── DELETE {id}/sections/{sectionId} — Eliminar sección ──────────────────
    [HttpDelete("{id:guid}/sections/{sectionId:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> DeleteSection(Guid id, Guid sectionId)
    {
        var section = await _db.TemplateSections
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.TemplateId == id);

        if (section == null)
            return NotFound(new { success = false, message = "Sección no encontrada." });

        // Remove options, then questions, then section
        foreach (var q in section.Questions)
            _db.TemplateQuestionOptions.RemoveRange(q.Options);
        _db.TemplateQuestions.RemoveRange(section.Questions);
        _db.TemplateSections.Remove(section);

        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Sección eliminada." });
    }

    // ── POST {id}/sections/{sectionId}/questions — Agregar pregunta ──────────
    [HttpPost("{id:guid}/sections/{sectionId:guid}/questions")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddQuestion(Guid id, Guid sectionId, [FromBody] AddQuestionRequest req)
    {
        var section = await _db.TemplateSections
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.TemplateId == id);

        if (section == null)
            return NotFound(new { success = false, message = "Sección no encontrada." });

        if (!Enum.TryParse<QuestionType>(req.QuestionType, true, out var qType))
            return BadRequest(new { success = false, message = $"Tipo de pregunta inválido: {req.QuestionType}" });

        var question = new TemplateQuestion
        {
            SectionId    = sectionId,
            QuestionText = req.QuestionText,
            QuestionType = qType,
            OrderIndex   = req.OrderIndex ?? (section.Questions.Count + 1),
            IsRequired   = req.IsRequired ?? true,
            IsCritical   = req.IsCritical ?? false,
            Weight       = req.Weight ?? 1.0m,
            MinPhotos    = req.MinPhotos ?? 0,
            MaxPhotos    = req.MaxPhotos ?? 10,
            TenantId     = GetTenantId(),
            CreatedBy    = GetUserId()
        };

        _db.TemplateQuestions.Add(question);

        // Add options if provided
        if (req.Options != null)
        {
            var idx = 0;
            foreach (var opt in req.Options)
            {
                _db.TemplateQuestionOptions.Add(new TemplateQuestionOption
                {
                    QuestionId      = question.Id,
                    Label           = opt.Label,
                    Value           = opt.Value ?? opt.Label,
                    OrderIndex      = idx++,
                    IsFailureOption = opt.IsFailureOption ?? false,
                    Score           = opt.Score ?? 1.0m
                });
            }
        }

        await _db.SaveChangesAsync();

        return CreatedData(new { question.Id, question.QuestionText, question.OrderIndex },
            $"/api/templates/{id}");
    }

    // ── DELETE {id}/sections/{sectionId}/questions/{questionId} ───────────────
    [HttpDelete("{id:guid}/sections/{sectionId:guid}/questions/{questionId:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> DeleteQuestion(Guid id, Guid sectionId, Guid questionId)
    {
        var question = await _db.TemplateQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.SectionId == sectionId);

        if (question == null)
            return NotFound(new { success = false, message = "Pregunta no encontrada." });

        _db.TemplateQuestionOptions.RemoveRange(question.Options);
        _db.TemplateQuestions.Remove(question);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Pregunta eliminada." });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────
public record CreateTemplateRequest(
    string Name, string? Description, string? TemplateType,
    decimal? PassingScore, bool RequireGeolocation, bool RequireSignature);

public record UpdateTemplateRequest(
    string? Name, string? Description, string? TemplateType,
    decimal? PassingScore, bool? RequireGeolocation, bool? RequireSignature);

public record AddSectionRequest(
    string Title, string? Description, int? OrderIndex, decimal? Weight, bool? IsRequired);

public record AddQuestionOptionRequest(
    string Label, string? Value, bool? IsFailureOption, decimal? Score);

public record AddQuestionRequest(
    string QuestionText, string QuestionType, int? OrderIndex,
    bool? IsRequired, bool? IsCritical, decimal? Weight,
    int? MinPhotos, int? MaxPhotos,
    List<AddQuestionOptionRequest>? Options);
