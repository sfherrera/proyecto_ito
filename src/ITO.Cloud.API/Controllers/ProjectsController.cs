using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Application.Features.Projects.Commands;
using ITO.Cloud.Application.Features.Projects.DTOs;
using ITO.Cloud.Application.Features.Projects.Queries;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de proyectos / obras</summary>
public class ProjectsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] Guid? companyId = null,
        [FromQuery] string? status = null) =>
        OkPaginated(await Mediator.Send(new GetProjectsQuery(page, pageSize, search, companyId, status)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        OkData(await Mediator.Send(new GetProjectByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto) =>
        CreatedData(await Mediator.Send(new CreateProjectCommand(
            dto.CompanyId, dto.Code, dto.Name, dto.ProjectType,
            dto.Description, dto.Address, dto.City, dto.Region,
            dto.StartDate, dto.EstimatedEndDate, dto.TotalUnits,
            dto.ItoManagerId, dto.MandanteName, dto.MandanteEmail,
            dto.ConstructionPermit, dto.Notes)));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto) =>
        OkData(await Mediator.Send(new UpdateProjectCommand(
            id, dto.Code, dto.Name, dto.ProjectType, dto.Status,
            dto.Description, dto.Address, dto.City, dto.Region,
            dto.StartDate, dto.EstimatedEndDate, dto.TotalUnits,
            dto.ItoManagerId, dto.MandanteName, dto.IsActive, dto.Notes)));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteProjectCommand(id));
        return Ok(new { success = true, message = "Proyecto eliminado." });
    }

    [HttpGet("{id:guid}/stages")]
    public async Task<IActionResult> GetStages(Guid id) =>
        OkData(await Mediator.Send(new GetProjectStagesQuery(id)));

    [HttpGet("{id:guid}/sectors")]
    public async Task<IActionResult> GetSectors(Guid id) =>
        OkData(await Mediator.Send(new GetProjectSectorsQuery(id)));

    [HttpGet("{id:guid}/units")]
    public async Task<IActionResult> GetUnits(Guid id, [FromQuery] Guid? sectorId = null) =>
        OkData(await Mediator.Send(new GetProjectUnitsQuery(id, sectorId)));

    // ── Stages CRUD ────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/stages")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> CreateStage(Guid id, [FromBody] CreateStageDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var stage = new ProjectStage
        {
            TenantId    = currentUser.TenantId,
            ProjectId   = id,
            Name        = dto.Name,
            Description = dto.Description,
            OrderIndex  = dto.OrderIndex,
            Status      = dto.Status ?? "pendiente",
            StartDate   = dto.StartDate,
            EndDate     = dto.EndDate,
            CreatedBy   = currentUser.UserId
        };
        db.ProjectStages.Add(stage);
        await db.SaveChangesAsync();
        return CreatedData(new ProjectStageDto(stage.Id, stage.ProjectId, stage.Name,
            stage.Status, stage.OrderIndex, stage.StartDate, stage.EndDate));
    }

    [HttpPut("{id:guid}/stages/{stageId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> UpdateStage(Guid id, Guid stageId, [FromBody] UpdateStageDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var stage = await db.ProjectStages.FirstOrDefaultAsync(s => s.Id == stageId && s.ProjectId == id);
        if (stage == null) return NotFound(new { success = false, message = "Etapa no encontrada." });

        stage.Name        = dto.Name;
        stage.Description = dto.Description;
        stage.OrderIndex  = dto.OrderIndex;
        stage.Status      = dto.Status ?? stage.Status;
        stage.StartDate   = dto.StartDate;
        stage.EndDate     = dto.EndDate;
        stage.UpdatedBy   = currentUser.UserId;
        stage.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return OkData(new ProjectStageDto(stage.Id, stage.ProjectId, stage.Name,
            stage.Status, stage.OrderIndex, stage.StartDate, stage.EndDate));
    }

    [HttpDelete("{id:guid}/stages/{stageId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> DeleteStage(Guid id, Guid stageId,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var stage = await db.ProjectStages.FirstOrDefaultAsync(s => s.Id == stageId && s.ProjectId == id);
        if (stage == null) return NotFound(new { success = false, message = "Etapa no encontrada." });

        stage.IsDeleted = true;
        stage.DeletedAt = DateTime.UtcNow;
        stage.DeletedBy = currentUser.UserId;
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Etapa eliminada." });
    }

    // ── Sectors CRUD ───────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/sectors")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> CreateSector(Guid id, [FromBody] CreateSectorDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var sector = new ProjectSector
        {
            TenantId       = currentUser.TenantId,
            ProjectId      = id,
            Name           = dto.Name,
            SectorType     = dto.SectorType ?? "sector",
            OrderIndex     = dto.OrderIndex,
            ParentSectorId = dto.ParentSectorId,
            CreatedBy      = currentUser.UserId
        };
        db.ProjectSectors.Add(sector);
        await db.SaveChangesAsync();
        return CreatedData(new ProjectSectorDto(sector.Id, sector.ProjectId,
            sector.ParentSectorId, sector.Name, sector.SectorType, sector.OrderIndex));
    }

    [HttpPut("{id:guid}/sectors/{sectorId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> UpdateSector(Guid id, Guid sectorId, [FromBody] UpdateSectorDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var sector = await db.ProjectSectors.FirstOrDefaultAsync(s => s.Id == sectorId && s.ProjectId == id);
        if (sector == null) return NotFound(new { success = false, message = "Sector no encontrado." });

        sector.Name           = dto.Name;
        sector.SectorType     = dto.SectorType ?? sector.SectorType;
        sector.OrderIndex     = dto.OrderIndex;
        sector.ParentSectorId = dto.ParentSectorId;
        sector.UpdatedBy      = currentUser.UserId;
        sector.UpdatedAt      = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return OkData(new ProjectSectorDto(sector.Id, sector.ProjectId,
            sector.ParentSectorId, sector.Name, sector.SectorType, sector.OrderIndex));
    }

    [HttpDelete("{id:guid}/sectors/{sectorId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> DeleteSector(Guid id, Guid sectorId,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var sector = await db.ProjectSectors.FirstOrDefaultAsync(s => s.Id == sectorId && s.ProjectId == id);
        if (sector == null) return NotFound(new { success = false, message = "Sector no encontrado." });

        sector.IsDeleted = true;
        sector.DeletedAt = DateTime.UtcNow;
        sector.DeletedBy = currentUser.UserId;
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Sector eliminado." });
    }

    // ── Units CRUD ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/units")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> CreateUnit(Guid id, [FromBody] CreateUnitDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var unit = new ProjectUnit
        {
            TenantId  = currentUser.TenantId,
            ProjectId = id,
            UnitCode  = dto.UnitCode,
            UnitType  = dto.UnitType ?? "departamento",
            SectorId  = dto.SectorId,
            Floor     = dto.Floor,
            SurfaceM2 = dto.SurfaceM2,
            Status    = dto.Status ?? "construccion",
            CreatedBy = currentUser.UserId
        };
        db.ProjectUnits.Add(unit);
        await db.SaveChangesAsync();
        return CreatedData(new ProjectUnitDto(unit.Id, unit.ProjectId, unit.SectorId,
            unit.UnitCode, unit.UnitType, unit.Floor, unit.SurfaceM2, unit.Status));
    }

    [HttpPut("{id:guid}/units/{unitId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> UpdateUnit(Guid id, Guid unitId, [FromBody] UpdateUnitDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var unit = await db.ProjectUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.ProjectId == id);
        if (unit == null) return NotFound(new { success = false, message = "Unidad no encontrada." });

        unit.UnitCode  = dto.UnitCode;
        unit.UnitType  = dto.UnitType ?? unit.UnitType;
        unit.SectorId  = dto.SectorId;
        unit.Floor     = dto.Floor;
        unit.SurfaceM2 = dto.SurfaceM2;
        unit.Status    = dto.Status ?? unit.Status;
        unit.UpdatedBy = currentUser.UserId;
        unit.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return OkData(new ProjectUnitDto(unit.Id, unit.ProjectId, unit.SectorId,
            unit.UnitCode, unit.UnitType, unit.Floor, unit.SurfaceM2, unit.Status));
    }

    [HttpDelete("{id:guid}/units/{unitId:guid}")]
    [Authorize(Roles = Roles.Operations)]
    public async Task<IActionResult> DeleteUnit(Guid id, Guid unitId,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        var unit = await db.ProjectUnits.FirstOrDefaultAsync(u => u.Id == unitId && u.ProjectId == id);
        if (unit == null) return NotFound(new { success = false, message = "Unidad no encontrada." });

        unit.IsDeleted = true;
        unit.DeletedAt = DateTime.UtcNow;
        unit.DeletedBy = currentUser.UserId;
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Unidad eliminada." });
    }

    // ── Members ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id,
        [FromServices] IApplicationDbContext db)
    {
        var members = await db.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == id && m.IsActive)
            .Include(m => m.User)
            .OrderBy(m => m.ProjectRole)
            .Select(m => new
            {
                m.Id,
                m.UserId,
                FullName    = m.User.FirstName + " " + m.User.LastName,
                m.User.Email,
                m.ProjectRole,
                m.IsActive,
                m.AssignedAt
            })
            .ToListAsync();

        return OkData(members);
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberDto dto,
        [FromServices] IApplicationDbContext db, [FromServices] ICurrentUserService currentUser)
    {
        // Check if user is already a member
        var exists = await db.ProjectMembers
            .AnyAsync(m => m.ProjectId == id && m.UserId == dto.UserId && m.IsActive);
        if (exists)
            return BadRequest(new { success = false, message = "El usuario ya es miembro del proyecto." });

        var member = new ProjectMember
        {
            TenantId    = currentUser.TenantId,
            ProjectId   = id,
            UserId      = dto.UserId,
            ProjectRole = dto.ProjectRole ?? "inspector",
            IsActive    = true,
            AssignedAt  = DateTime.UtcNow,
            AssignedBy  = currentUser.UserId
        };

        db.ProjectMembers.Add(member);
        await db.SaveChangesAsync();

        return CreatedData(new { member.Id, member.UserId, member.ProjectRole });
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId,
        [FromServices] IApplicationDbContext db)
    {
        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.ProjectId == id);

        if (member == null)
            return NotFound(new { success = false, message = "Miembro no encontrado." });

        member.IsActive = false;
        await db.SaveChangesAsync();

        return Ok(new { success = true, message = "Miembro removido del proyecto." });
    }
}

// ── DTOs for Stage/Sector/Unit CRUD ────────────────────────────────────────
public record CreateStageDto(string Name, string? Description = null, int OrderIndex = 0,
    string? Status = null, DateOnly? StartDate = null, DateOnly? EndDate = null);

public record UpdateStageDto(string Name, string? Description = null, int OrderIndex = 0,
    string? Status = null, DateOnly? StartDate = null, DateOnly? EndDate = null);

public record CreateSectorDto(string Name, string? SectorType = null, int OrderIndex = 0,
    Guid? ParentSectorId = null);

public record UpdateSectorDto(string Name, string? SectorType = null, int OrderIndex = 0,
    Guid? ParentSectorId = null);

public record CreateUnitDto(string UnitCode, string? UnitType = null, Guid? SectorId = null,
    int? Floor = null, decimal? SurfaceM2 = null, string? Status = null);

public record UpdateUnitDto(string UnitCode, string? UnitType = null, Guid? SectorId = null,
    int? Floor = null, decimal? SurfaceM2 = null, string? Status = null);

public record AddProjectMemberDto(Guid UserId, string? ProjectRole = null);
