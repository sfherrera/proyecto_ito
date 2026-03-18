using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Inspections;

public class InspectionAnswer : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid InspectionId { get; set; }
    public Guid QuestionId { get; set; }
    public string? AnswerValue { get; set; }           // texto/JSON de la respuesta
    public Guid? SelectedOptionId { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public bool? IsConforming { get; set; }            // null = N/A, true = conforme, false = no conforme
    public bool IsNa { get; set; } = false;
    public decimal? Score { get; set; }
    public string? Notes { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public Guid? AnsweredBy { get; set; }

    // Navegación
    public Inspection Inspection { get; set; } = null!;
    public ICollection<InspectionEvidence> Evidence { get; set; } = [];
}
