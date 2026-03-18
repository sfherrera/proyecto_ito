using ITO.Cloud.Application.Features.Projects.Commands;
using ITO.Cloud.Application.Features.Projects.DTOs;
using ITO.Cloud.Application.Features.Projects.Queries;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto) =>
        CreatedData(await Mediator.Send(new CreateProjectCommand(
            dto.CompanyId, dto.Code, dto.Name, dto.ProjectType,
            dto.Description, dto.Address, dto.City, dto.Region,
            dto.StartDate, dto.EstimatedEndDate, dto.TotalUnits,
            dto.ItoManagerId, dto.MandanteName, dto.MandanteEmail,
            dto.ConstructionPermit, dto.Notes)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto) =>
        OkData(await Mediator.Send(new UpdateProjectCommand(
            id, dto.Code, dto.Name, dto.ProjectType, dto.Status,
            dto.Description, dto.Address, dto.City, dto.Region,
            dto.StartDate, dto.EstimatedEndDate, dto.TotalUnits,
            dto.ItoManagerId, dto.MandanteName, dto.IsActive, dto.Notes)));

    [HttpDelete("{id:guid}")]
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
}
