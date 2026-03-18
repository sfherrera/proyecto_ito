using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Domain.Interfaces.Services;

/// <summary>
/// Interfaz desacoplada para integración futura de IA.
/// Actualmente implementada por AIServiceStub en Infrastructure.
/// Cuando se integre OpenAI, solo cambia la implementación.
/// </summary>
public interface IAIService
{
    Task<string> GenerateObservationDescriptionAsync(string context, CancellationToken cancellationToken = default);
    Task<string> SummarizeInspectionAsync(string inspectionJson, CancellationToken cancellationToken = default);
    Task<ObservationSeverity> ClassifySeverityAsync(string description, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> SemanticSearchAsync(string query, string contextJson, CancellationToken cancellationToken = default);
}
