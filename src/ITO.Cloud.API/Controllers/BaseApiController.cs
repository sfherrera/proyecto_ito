using ITO.Cloud.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITO.Cloud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult OkPaginated<T>(PaginatedList<T> list)
    {
        return Ok(new
        {
            success = true,
            data    = list.Items,
            pagination = new PaginationMeta
            {
                Page       = list.Page,
                PageSize   = list.PageSize,
                TotalCount = list.TotalCount
            }
        });
    }

    protected IActionResult OkData<T>(T data) =>
        Ok(new { success = true, data });

    protected IActionResult CreatedData<T>(T data, string? location = null) =>
        Created(location ?? string.Empty, new { success = true, data });
}
