namespace ITO.Cloud.Domain.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Email { get; }
    string FullName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}
