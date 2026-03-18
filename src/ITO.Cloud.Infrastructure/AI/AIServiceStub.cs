using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ITO.Cloud.Infrastructure.AI;

/// <summary>
/// Stub de IA — implementación vacía hasta que se integre OpenAI u otro proveedor.
/// Al integrar, reemplazar este stub con OpenAIService y reasignar en DI.
/// </summary>
public class AIServiceStub : IAIService
{
    private readonly ILogger<AIServiceStub> _logger;

    public AIServiceStub(ILogger<AIServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateObservationDescriptionAsync(string context, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("AIServiceStub: GenerateObservationDescription called (not implemented)");
        return Task.FromResult(string.Empty);
    }

    public Task<string> SummarizeInspectionAsync(string inspectionJson, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("AIServiceStub: SummarizeInspection called (not implemented)");
        return Task.FromResult(string.Empty);
    }

    public Task<ObservationSeverity> ClassifySeverityAsync(string description, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("AIServiceStub: ClassifySeverity called (not implemented)");
        return Task.FromResult(ObservationSeverity.Media);
    }

    public Task<IEnumerable<string>> SemanticSearchAsync(string query, string contextJson, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("AIServiceStub: SemanticSearch called (not implemented)");
        return Task.FromResult(Enumerable.Empty<string>());
    }
}
