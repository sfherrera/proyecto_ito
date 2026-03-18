using ITO.Cloud.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ITO.Cloud.Infrastructure.Identity;

public class TenantContextService : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
