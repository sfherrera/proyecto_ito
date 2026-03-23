using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Domain.Entities.Inspections;

public class Inspection : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid TemplateId { get; set; }
    public int TemplateVersion { get; set; } = 1;
    public Guid? StageId { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? UnitId { get; set; }
    public string Code { get; set; } = string.Empty;       // INS-2026-0001
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InspectionType { get; set; } = "ordinaria";
    public InspectionStatus Status { get; set; } = InspectionStatus.Programada;
    public InspectionPriority Priority { get; set; } = InspectionPriority.Normal;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? AssignedById { get; set; }
    public Guid? SupervisorId { get; set; }
    public Guid? ContractorId { get; set; }
    public Guid? SpecialtyId { get; set; }

    // Resultados
    public decimal? Score { get; set; }
    public decimal? PassingScore { get; set; }
    public bool? Passed { get; set; }
    public int TotalQuestions { get; set; } = 0;
    public int AnsweredQuestions { get; set; } = 0;
    public int ConformingCount { get; set; } = 0;
    public int NonConformingCount { get; set; } = 0;
    public int NaCount { get; set; } = 0;

    // Geolocalización y condiciones
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? WeatherConditions { get; set; }
    public decimal? Temperature { get; set; }

    // Sync offline
    public bool IsOfflineCreated { get; set; } = false;
    public string? SyncId { get; set; }     // ID temporal generado en Android
    public string? Notes { get; set; }

    // Navegación
    public ApplicationUser? AssignedTo { get; set; }
    public Contractor? Contractor { get; set; }
    public ICollection<InspectionAnswer> Answers { get; set; } = [];
    public ICollection<InspectionEvidence> Evidence { get; set; } = [];
}
