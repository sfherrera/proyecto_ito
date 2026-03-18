namespace ITO.Cloud.Application.Features.Projects.DTOs;

public record ProjectDto(
    Guid Id, Guid CompanyId, string? CompanyName, string Code, string Name,
    string? Description, string ProjectType, string Status,
    string? City, string? Region, string? Address,
    decimal? Latitude, decimal? Longitude,
    DateOnly? StartDate, DateOnly? EstimatedEndDate,
    int? TotalUnits, Guid? ItoManagerId, string? MandanteName,
    bool IsActive, DateTime CreatedAt);

public record CreateProjectDto(
    Guid CompanyId, string Code, string Name,
    string ProjectType = "edificio",
    string? Description = null, string? Address = null,
    string? City = null, string? Region = null,
    DateOnly? StartDate = null, DateOnly? EstimatedEndDate = null,
    int? TotalUnits = null, Guid? ItoManagerId = null,
    string? MandanteName = null, string? MandanteEmail = null,
    string? ConstructionPermit = null, string? Notes = null);

public record UpdateProjectDto(
    string Code, string Name, string ProjectType, string Status,
    string? Description, string? Address, string? City, string? Region,
    DateOnly? StartDate, DateOnly? EstimatedEndDate, int? TotalUnits,
    Guid? ItoManagerId, string? MandanteName, bool IsActive, string? Notes);

public record ProjectStageDto(Guid Id, Guid ProjectId, string Name, string Status, int OrderIndex, DateOnly? StartDate, DateOnly? EndDate);
public record ProjectSectorDto(Guid Id, Guid ProjectId, Guid? ParentSectorId, string Name, string SectorType, int OrderIndex);
public record ProjectUnitDto(Guid Id, Guid ProjectId, Guid? SectorId, string UnitCode, string UnitType, int? Floor, decimal? SurfaceM2, string Status);
public record SpecialtyDto(Guid Id, string Name, string? Code, string? Color, bool IsActive);
public record ContractorDto(Guid Id, string Name, string? Rut, string? ContactName, string? ContactEmail, bool IsActive);
