using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de contratistas</summary>
public class ContractorsController : BaseApiController
{
    private readonly ApplicationDbContext _db;

    public ContractorsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100,
        [FromQuery] string? search = null, [FromQuery] Guid? companyId = null, [FromQuery] bool? isActive = null)
    {
        var q = _db.Contractors.AsNoTracking()
            .Include(c => c.Company);

        IQueryable<Domain.Entities.Projects.Contractor> filtered = q;

        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(c => c.Name.Contains(search)
                || (c.Rut != null && c.Rut.Contains(search))
                || (c.ContactName != null && c.ContactName.Contains(search)));

        if (companyId.HasValue)
            filtered = filtered.Where(c => c.CompanyId == companyId.Value);

        if (isActive.HasValue)
            filtered = filtered.Where(c => c.IsActive == isActive.Value);

        var totalCount = await filtered.CountAsync();

        var items = await filtered
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Rut,
                c.ContactName,
                c.ContactEmail,
                c.ContactPhone,
                c.CompanyId,
                CompanyName = c.Company != null ? c.Company.Name : null,
                c.IsActive
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
        var contractor = await _db.Contractors.AsNoTracking()
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contractor == null)
            return NotFound(new { success = false, message = "Contratista no encontrado." });

        return OkData(new
        {
            contractor.Id,
            contractor.Name,
            contractor.Rut,
            contractor.ContactName,
            contractor.ContactEmail,
            contractor.ContactPhone,
            contractor.CompanyId,
            CompanyName = contractor.Company?.Name,
            contractor.Notes,
            contractor.IsActive,
            contractor.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Create([FromBody] CreateContractorRequest dto)
    {
        var contractor = new Domain.Entities.Projects.Contractor
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Rut = dto.Rut,
            ContactName = dto.ContactName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            CompanyId = dto.CompanyId,
            Notes = dto.Notes,
            IsActive = true
        };

        _db.Contractors.Add(contractor);
        await _db.SaveChangesAsync();

        return CreatedData(new { contractor.Id, contractor.Name, contractor.Rut, contractor.IsActive });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractorRequest dto)
    {
        var contractor = await _db.Contractors.FindAsync(id);
        if (contractor == null)
            return NotFound(new { success = false, message = "Contratista no encontrado." });

        contractor.Name = dto.Name;
        contractor.Rut = dto.Rut;
        contractor.ContactName = dto.ContactName;
        contractor.ContactEmail = dto.ContactEmail;
        contractor.ContactPhone = dto.ContactPhone;
        contractor.CompanyId = dto.CompanyId;
        contractor.Notes = dto.Notes;
        contractor.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return OkData(new { contractor.Id, contractor.Name, contractor.Rut, contractor.IsActive });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var contractor = await _db.Contractors.FindAsync(id);
        if (contractor == null)
            return NotFound(new { success = false, message = "Contratista no encontrado." });

        _db.Contractors.Remove(contractor);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Contratista eliminado." });
    }
}

public record CreateContractorRequest(string Name, string? Rut, string? ContactName, string? ContactEmail, string? ContactPhone, Guid? CompanyId, string? Notes);
public record UpdateContractorRequest(string Name, string? Rut, string? ContactName, string? ContactEmail, string? ContactPhone, Guid? CompanyId, string? Notes, bool IsActive);
