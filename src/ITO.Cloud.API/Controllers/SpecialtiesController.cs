using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de especialidades</summary>
public class SpecialtiesController : BaseApiController
{
    private readonly ApplicationDbContext _db;

    public SpecialtiesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100,
        [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        var q = _db.Specialties.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => s.Name.Contains(search) || (s.Code != null && s.Code.Contains(search)));

        if (isActive.HasValue)
            q = q.Where(s => s.IsActive == isActive.Value);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Code,
                s.Description,
                s.Color,
                s.IsActive
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = items,
            pagination = new { page, pageSize, totalCount }
        });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var specialty = await _db.Specialties.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (specialty == null)
            return NotFound(new { success = false, message = "Especialidad no encontrada." });

        return OkData(new
        {
            specialty.Id,
            specialty.Name,
            specialty.Code,
            specialty.Description,
            specialty.Color,
            specialty.IsActive,
            specialty.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Create([FromBody] CreateSpecialtyRequest dto)
    {
        var specialty = new Domain.Entities.Projects.Specialty
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Color = dto.Color,
            IsActive = true
        };

        _db.Specialties.Add(specialty);
        await _db.SaveChangesAsync();

        return CreatedData(new { specialty.Id, specialty.Name, specialty.Code, specialty.Color, specialty.IsActive });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialtyRequest dto)
    {
        var specialty = await _db.Specialties.FindAsync(id);
        if (specialty == null)
            return NotFound(new { success = false, message = "Especialidad no encontrada." });

        specialty.Name = dto.Name;
        specialty.Code = dto.Code;
        specialty.Description = dto.Description;
        specialty.Color = dto.Color;
        specialty.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return OkData(new { specialty.Id, specialty.Name, specialty.Code, specialty.Color, specialty.IsActive });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var specialty = await _db.Specialties.FindAsync(id);
        if (specialty == null)
            return NotFound(new { success = false, message = "Especialidad no encontrada." });

        _db.Specialties.Remove(specialty);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Especialidad eliminada." });
    }
}

public record CreateSpecialtyRequest(string Name, string? Code, string? Description, string? Color);
public record UpdateSpecialtyRequest(string Name, string? Code, string? Description, string? Color, bool IsActive);
