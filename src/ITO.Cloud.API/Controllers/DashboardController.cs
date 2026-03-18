using ITO.Cloud.Application.Features.Observations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ITO.Cloud.API.Controllers;

/// <summary>KPIs y métricas ejecutivas</summary>
public class DashboardController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? projectId = null) =>
        OkData(await Mediator.Send(new GetDashboardQuery(projectId)));
}
