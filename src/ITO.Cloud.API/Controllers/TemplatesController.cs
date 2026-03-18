using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Consulta de plantillas de inspección</summary>
public class TemplatesController : BaseApiController
{
    private readonly ApplicationDbContext _db;

    public TemplatesController(ApplicationDbContext db) => _db = db;

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
}
