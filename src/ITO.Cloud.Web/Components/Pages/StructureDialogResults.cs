namespace ITO.Cloud.Web.Components.Pages;

public record StageDialogResult(string Name, string? Description, int OrderIndex, string Status, DateOnly? StartDate, DateOnly? EndDate);
public record SectorDialogResult(string Name, string SectorType, int OrderIndex, Guid? ParentSectorId);
public record UnitDialogResult(string UnitCode, string UnitType, Guid? SectorId, int? Floor, decimal? SurfaceM2, string Status);
