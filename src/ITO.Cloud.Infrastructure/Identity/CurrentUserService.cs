using ITO.Cloud.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ITO.Cloud.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public Guid TenantId
    {
        get
        {
            var value = User?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public string FullName => User?.FindFirstValue("full_name")
                           ?? User?.FindFirstValue(ClaimTypes.Name)
                           ?? string.Empty;

    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
