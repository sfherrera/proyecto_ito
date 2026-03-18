namespace ITO.Cloud.Domain.Interfaces.Services;

public record InspectionReportData(
    string InspectionCode,
    string InspectionTitle,
    string ProjectCode,
    string ProjectName,
    string? InspectorName,
    DateTime? ScheduledDate,
    DateTime? FinishedAt,
    string Status,
    decimal? Score,
    bool?   Passed,
    string? WeatherConditions,
    string? Notes,
    IList<SectionReportData> Sections
);

public record SectionReportData(
    string SectionName,
    int    OrderIndex,
    IList<AnswerReportData> Answers
);

public record AnswerReportData(
    string QuestionText,
    bool   IsCritical,
    string? AnswerValue,
    bool?  IsConforming,
    bool   IsNa,
    string? Notes
);

public record ObservationsReportData(
    string   ProjectCode,
    string   ProjectName,
    DateTime GeneratedAt,
    IList<ObsRow> Observations
);

public record ObsRow(
    string Code,
    string Title,
    string Severity,
    string Status,
    string? ContractorName,
    DateOnly? DueDate,
    bool IsOverdue,
    string? ClosedAt
);

public interface IReportService
{
    /// <summary>Genera el informe PDF de una inspección.</summary>
    byte[] GenerateInspectionPdf(InspectionReportData data);

    /// <summary>Genera un Excel con el listado de observaciones del proyecto.</summary>
    byte[] GenerateObservationsExcel(ObservationsReportData data);
}
