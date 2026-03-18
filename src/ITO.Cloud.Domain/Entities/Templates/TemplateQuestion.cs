using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Domain.Entities.Templates;

public class TemplateQuestion : TenantEntity
{
    public Guid SectionId { get; set; }
    public Guid? ParentQuestionId { get; set; }    // para preguntas condicionales
    public string? TriggerValue { get; set; }       // valor que activa esta pregunta
    public string QuestionText { get; set; } = string.Empty;
    public string? Description { get; set; }        // texto de ayuda
    public QuestionType QuestionType { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsRequired { get; set; } = true;
    public bool IsCritical { get; set; } = false;  // si falla genera NC automática
    public decimal Weight { get; set; } = 1.0m;
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int MinPhotos { get; set; } = 0;
    public int MaxPhotos { get; set; } = 10;
    public string? Placeholder { get; set; }
    public string? ValidationRegex { get; set; }
    public string? ValidationMessage { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public TemplateSection Section { get; set; } = null!;
    public TemplateQuestion? ParentQuestion { get; set; }
    public ICollection<TemplateQuestion> ConditionalQuestions { get; set; } = [];
    public ICollection<TemplateQuestionOption> Options { get; set; } = [];
}
