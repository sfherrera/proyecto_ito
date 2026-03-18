using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Templates;

public class TemplateSection : TenantEntity
{
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsRequired { get; set; } = true;
    public decimal Weight { get; set; } = 1.0m;
    public bool IsActive { get; set; } = true;

    // Navegación
    public InspectionTemplate Template { get; set; } = null!;
    public ICollection<TemplateQuestion> Questions { get; set; } = [];
}
