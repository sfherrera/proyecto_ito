using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Identity;

public class SequenceCounter : BaseEntity
{
    public Guid TenantId { get; set; }
    public string EntityType { get; set; } = string.Empty;  // inspection, observation, reinspection
    public string Prefix { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LastValue { get; set; } = 0;
}
