namespace ITO.Cloud.Domain.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsAuthenticated { get; }
}
