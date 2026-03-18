using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Domain.Entities.Templates;

public class InspectionTemplate : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TemplateType { get; set; }      // recepcion_preliminar, calidad, seguridad...
    public Guid? SpecialtyId { get; set; }
    public TemplateStatus Status { get; set; } = TemplateStatus.Borrador;
    public int CurrentVersion { get; set; } = 1;
    public bool IsGlobal { get; set; } = false;    // disponible para todo el tenant
    public bool AllowPartialSave { get; set; } = true;
    public bool RequireGeolocation { get; set; } = false;
    public bool RequireSignature { get; set; } = false;
    public decimal? PassingScore { get; set; }     // puntaje mínimo para aprobar

    // Navegación
    public ICollection<TemplateSection> Sections { get; set; } = [];
    public ICollection<TemplateVersion> Versions { get; set; } = [];
}
