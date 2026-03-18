using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Inspections;

public class InspectionEvidence : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid InspectionId { get; set; }
    public Guid? AnswerId { get; set; }
    public Guid? ObservationId { get; set; }
    public string FileType { get; set; } = "photo";     // photo, video, audio, document, signature
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // ruta en MinIO
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Caption { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? TakenAt { get; set; }
    public bool IsOfflineUploaded { get; set; } = false;
    public Guid CreatedBy { get; set; }

    // Navegación
    public Inspection Inspection { get; set; } = null!;
    public InspectionAnswer? Answer { get; set; }
}
