using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Templates;

public class TemplateQuestionOption : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 0;
    public bool IsFailureOption { get; set; } = false;  // si se selecciona = no conforme
    public decimal Score { get; set; } = 1.0m;

    public TemplateQuestion Question { get; set; } = null!;
}
