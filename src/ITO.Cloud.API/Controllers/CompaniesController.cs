using ITO.Cloud.Application.Features.Companies.Commands;
using ITO.Cloud.Application.Features.Companies.DTOs;
using ITO.Cloud.Application.Features.Companies.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de empresas clientes</summary>
public class CompaniesController : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? isActive = null) =>
        OkPaginated(await Mediator.Send(new GetCompaniesQuery(page, pageSize, search, isActive)));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> GetById(Guid id) =>
        OkData(await Mediator.Send(new GetCompanyByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto) =>
        CreatedData(await Mediator.Send(new CreateCompanyCommand(
            dto.Name, dto.Rut, dto.BusinessName, dto.CompanyType,
            dto.Address, dto.City, dto.Region, dto.Phone, dto.Email,
            dto.Website, dto.Notes)));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto) =>
        OkData(await Mediator.Send(new UpdateCompanyCommand(
            id, dto.Name, dto.Rut, dto.BusinessName, dto.CompanyType,
            dto.Address, dto.City, dto.Region, dto.Phone, dto.Email,
            dto.Website, dto.IsActive, dto.Notes)));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCompanyCommand(id));
        return Ok(new { success = true, message = "Empresa eliminada." });
    }
}
