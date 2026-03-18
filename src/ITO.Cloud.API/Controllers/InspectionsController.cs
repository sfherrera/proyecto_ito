using ITO.Cloud.Application.Features.Inspections.Commands;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Application.Features.Inspections.Queries;
using ITO.Cloud.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de inspecciones</summary>
public class InspectionsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? projectId = null, [FromQuery] InspectionStatus? status = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null) =>
        OkPaginated(await Mediator.Send(new GetInspectionsQuery(page, pageSize, projectId, assignedToId, status, fromDate, toDate)));

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] InspectionStatus? status = null) =>
        OkData(await Mediator.Send(new GetMyInspectionsQuery(status)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeAnswers = false) =>
        OkData(await Mediator.Send(new GetInspectionByIdQuery(id, includeAnswers)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto) =>
        CreatedData(await Mediator.Send(new CreateInspectionCommand(
            dto.ProjectId, dto.TemplateId, dto.Title, dto.ScheduledDate,
            dto.InspectionType, dto.Priority,
            dto.StageId, dto.SectorId, dto.UnitId,
            dto.AssignedToId, dto.ContractorId, dto.SpecialtyId,
            dto.Description, dto.ScheduledEndDate)));

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id) =>
        OkData(await Mediator.Send(new StartInspectionCommand(id)));

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitInspectionDto dto) =>
        OkData(await Mediator.Send(new SubmitInspectionCommand(
            id, dto.Answers, dto.Latitude, dto.Longitude,
            dto.WeatherConditions, dto.Notes)));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string? reason = null)
    {
        await Mediator.Send(new CancelInspectionCommand(id, reason));
        return Ok(new { success = true, message = "Inspección cancelada." });
    }
}
