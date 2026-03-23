using ITO.Cloud.Web.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ITO.Cloud.Web.Services;

// ── DTOs de lista (vistas resumidas) ─────────────────────────────────────────
public record CompanyDto(Guid Id, string Name, string? Rut, string CompanyType, bool IsActive);
public record ProjectDto(Guid Id, string Code, string Name, string? Status, string? CompanyName, string? StartDate);
public record InspectionDto(Guid Id, string Code, string Title, string Status, string? AssignedToName, DateTime? ScheduledDate, decimal? Score);
public record ObservationDto(Guid Id, string Code, string Title, string Status, string Severity, string? DueDate, bool IsOverdue);
public record DashboardDto(
    int TotalInspections, int InspectionsThisMonth,
    int OpenObservations, int CriticalObservations,
    int ClosedObservationsThisMonth, int OverdueObservations,
    double AverageClosingDays, double ComplianceRate,
    List<ChartDataPointDto>? InspectionsByWeek = null,
    List<ChartDataPointDto>? ObservationsBySeverity = null,
    List<ContractorComplianceItemDto>? ContractorCompliance = null);

public record ChartDataPointDto(string Label, int Value);
public record ContractorComplianceItemDto(string ContractorName, int Total, int Closed, double Rate);
public record TemplateItemDto(Guid Id, string Name, string? TemplateType, string Status);
public record UserDto(Guid Id, string FirstName, string LastName, string FullName, string Email, string? Rut, string? Position, bool IsActive, DateTime CreatedAt, List<string>? Roles);
public record SpecialtyDto(Guid Id, string Name, string? Code, string? Description, string? Color, bool IsActive);
public record ContractorDto(Guid Id, string Name, string? Rut, string? ContactName, string? ContactEmail, string? ContactPhone, Guid? CompanyId, string? CompanyName, bool IsActive);

// ── DTOs de documentos ───────────────────────────────────────────────────────
public record DocumentDto(Guid Id, Guid ProjectId, string? ProjectName, string Category, string Name,
    string? Description, string FileName, long? FileSizeBytes, string? MimeType, string? Version,
    bool IsActive, DateTime CreatedAt);

// ── DTOs de detalle ───────────────────────────────────────────────────────────
public record CompanyDetailDto(Guid Id, string Name, string? Rut, string? BusinessName,
    string CompanyType, string? Address, string? City, string? Region,
    string? Phone, string? Email, string? Website, bool IsActive, string? Notes, DateTime CreatedAt);

public record ProjectDetailDto(Guid Id, Guid CompanyId, string? CompanyName, string Code, string Name,
    string? Description, string ProjectType, string Status,
    string? City, string? Region, string? Address,
    string? StartDate, string? EstimatedEndDate,
    int? TotalUnits, string? MandanteName, bool IsActive, DateTime CreatedAt);

public record InspectionDetailDto(Guid Id, Guid ProjectId, string Code, string Title,
    string InspectionType, string Status, string Priority,
    DateTime ScheduledDate, DateTime? StartedAt, DateTime? FinishedAt,
    string? AssignedToName, string? ContractorName,
    decimal? Score, bool? Passed,
    int TotalQuestions, int AnsweredQuestions,
    int ConformingCount, int NonConformingCount, string? Notes, DateTime CreatedAt);

public record ObservationDetailDto(Guid Id, Guid ProjectId, string Code, string Title, string Description,
    string Severity, string Status, string? Category, string? LocationDescription,
    string? ContractorName, string? AssignedToName,
    DateTime DetectedAt, string? DueDate, DateTime? ClosedAt,
    bool IsRecurring, DateTime CreatedAt,
    List<ObservationHistoryItemDto>? History);

public record ObservationHistoryItemDto(string Action, string? PreviousStatus, string? NewStatus, string? Comment, DateTime CreatedAt);

// ── DTOs de estructura del proyecto ──────────────────────────────────────────
public record StageDto(Guid Id, Guid ProjectId, string Name, string Status, int OrderIndex, string? StartDate, string? EndDate);
public record SectorDto(Guid Id, Guid ProjectId, Guid? ParentSectorId, string Name, string SectorType, int OrderIndex);
public record UnitDto(Guid Id, Guid ProjectId, Guid? SectorId, string UnitCode, string UnitType, int? Floor, decimal? SurfaceM2, string Status);

// ── DTOs de plantilla detallada ─────────────────────────────────────────────
public record TemplateDetailDto(Guid Id, string Name, string? Description, string? TemplateType,
    string Status, int CurrentVersion, bool IsGlobal,
    bool AllowPartialSave, bool RequireGeolocation, bool RequireSignature,
    decimal? PassingScore, DateTime CreatedAt,
    List<TemplateSectionDto> Sections);

public record TemplateSectionDto(Guid Id, string Title, string? Description,
    int OrderIndex, bool IsRequired, decimal Weight,
    List<TemplateQuestionDto> Questions);

public record TemplateQuestionDto(Guid Id, string QuestionText, string? Description,
    string QuestionType, int OrderIndex, bool IsRequired, bool IsCritical,
    decimal Weight, int MinPhotos, int MaxPhotos,
    List<TemplateOptionDto>? Options);

public record TemplateOptionDto(Guid Id, string Label, string Value, bool IsFailureOption, decimal Score);

// ── DTOs de miembros del proyecto ───────────────────────────────────────────
public record ProjectMemberDto(Guid Id, Guid UserId, string FullName, string? Email, string ProjectRole, bool IsActive, DateTime AssignedAt);

// ── DTO auxiliar para respuestas de creación ────────────────────────────────
public record CreatedIdDto(Guid Id);

// ── Wrappers de respuesta de la API ──────────────────────────────────────────
// Para endpoints que devuelven un solo objeto: { "success": true, "data": {...} }
public record ApiResponse<T>(bool Success, T? Data, string? Message);

// Para endpoints paginados: { "success": true, "data": [...], "pagination": {...} }
public record ApiListResponse<T>(bool Success, List<T>? Data, string? Message);

public class ItoApiService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;

    public ItoApiService(HttpClient http, TokenStorageService tokenStorage)
    {
        _http         = http;
        _tokenStorage = tokenStorage;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public async Task<DashboardDto?> GetDashboardAsync()
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<DashboardDto>>("/api/dashboard");
        return resp?.Data;
    }

    // ── Companies ─────────────────────────────────────────────────────────────
    public async Task<List<CompanyDto>> GetCompaniesAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<CompanyDto>>(
            $"/api/companies?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<CompanyDetailDto>>($"/api/companies/{id}");
        return resp?.Data;
    }

    public async Task<bool> CreateCompanyAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/companies", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCompanyAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/companies/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCompanyAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/companies/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Projects ──────────────────────────────────────────────────────────────
    public async Task<List<ProjectDto>> GetProjectsAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<ProjectDto>>(
            $"/api/projects?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<ProjectDetailDto>>($"/api/projects/{id}");
        return resp?.Data;
    }

    public async Task<bool> CreateProjectAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/projects", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProjectAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/projects/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/projects/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Inspections ───────────────────────────────────────────────────────────
    public async Task<List<InspectionDto>> GetInspectionsAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<InspectionDto>>(
            $"/api/inspections?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<InspectionDetailDto?> GetInspectionByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<InspectionDetailDto>>($"/api/inspections/{id}");
        return resp?.Data;
    }

    public async Task<bool> CreateInspectionAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/inspections", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> StartInspectionAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsync($"/api/inspections/{id}/start", null);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> CancelInspectionAsync(Guid id, string? reason = null)
    {
        await AddAuthHeaderAsync();
        var url = string.IsNullOrEmpty(reason)
            ? $"/api/inspections/{id}/cancel"
            : $"/api/inspections/{id}/cancel?reason={Uri.EscapeDataString(reason)}";
        var r = await _http.PostAsync(url, null);
        return r.IsSuccessStatusCode;
    }

    // ── Observations ──────────────────────────────────────────────────────────
    public async Task<List<ObservationDto>> GetObservationsAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<ObservationDto>>(
            $"/api/observations?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<ObservationDetailDto?> GetObservationByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<ObservationDetailDto>>($"/api/observations/{id}");
        return resp?.Data;
    }

    public async Task<bool> CreateObservationAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/observations", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateObservationStatusAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PatchAsJsonAsync($"/api/observations/{id}/status", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> CloseObservationAsync(Guid id, string? comment = null)
    {
        await AddAuthHeaderAsync();
        var url = string.IsNullOrEmpty(comment)
            ? $"/api/observations/{id}/close"
            : $"/api/observations/{id}/close?comment={Uri.EscapeDataString(comment)}";
        var r = await _http.PostAsync(url, null);
        return r.IsSuccessStatusCode;
    }

    // ── Reports ──────────────────────────────────────────────────────────────
    public async Task<byte[]?> DownloadInspectionPdfAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetAsync($"/api/reports/inspections/{id}/pdf");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync();
    }

    public async Task<byte[]?> DownloadObservationsExcelAsync(Guid projectId)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetAsync($"/api/reports/projects/{projectId}/observations/excel");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<InspectionDto>> GetInspectionsByProjectAsync(Guid projectId, int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<InspectionDto>>(
            $"/api/inspections?projectId={projectId}&page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    // ── Templates ─────────────────────────────────────────────────────────────
    public async Task<List<TemplateItemDto>> GetTemplatesAsync()
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<TemplateItemDto>>>("/api/templates");
        return resp?.Data ?? [];
    }

    public async Task<TemplateDetailDto?> GetTemplateByIdAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<TemplateDetailDto>>($"/api/templates/{id}");
        return resp?.Data;
    }

    public async Task<Guid?> CreateTemplateAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/templates", payload);
        if (!r.IsSuccessStatusCode) return null;
        var resp = await r.Content.ReadFromJsonAsync<ApiResponse<CreatedIdDto>>();
        return resp?.Data?.Id;
    }

    public async Task<bool> UpdateTemplateAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/templates/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/templates/{id}");
        return r.IsSuccessStatusCode;
    }

    public async Task<Guid?> AddTemplateSectionAsync(Guid templateId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/templates/{templateId}/sections", payload);
        if (!r.IsSuccessStatusCode) return null;
        var resp = await r.Content.ReadFromJsonAsync<ApiResponse<CreatedIdDto>>();
        return resp?.Data?.Id;
    }

    public async Task<bool> DeleteTemplateSectionAsync(Guid templateId, Guid sectionId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/templates/{templateId}/sections/{sectionId}");
        return r.IsSuccessStatusCode;
    }

    public async Task<Guid?> AddTemplateQuestionAsync(Guid templateId, Guid sectionId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/templates/{templateId}/sections/{sectionId}/questions", payload);
        if (!r.IsSuccessStatusCode) return null;
        var resp = await r.Content.ReadFromJsonAsync<ApiResponse<CreatedIdDto>>();
        return resp?.Data?.Id;
    }

    public async Task<bool> DeleteTemplateQuestionAsync(Guid templateId, Guid sectionId, Guid questionId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/templates/{templateId}/sections/{sectionId}/questions/{questionId}");
        return r.IsSuccessStatusCode;
    }

    // ── Project Members ────────────────────────────────────────────────────────
    public async Task<List<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<ProjectMemberDto>>>($"/api/projects/{projectId}/members");
        return resp?.Data ?? [];
    }

    public async Task<bool> AddProjectMemberAsync(Guid projectId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/projects/{projectId}/members", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveProjectMemberAsync(Guid projectId, Guid memberId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/projects/{projectId}/members/{memberId}");
        return r.IsSuccessStatusCode;
    }

    // ── Users ─────────────────────────────────────────────────────────────────
    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<UserDto>>(
            $"/api/users?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateUserAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/users", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/users/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    // ── Specialties ────────────────────────────────────────────────────────────
    public async Task<List<SpecialtyDto>> GetSpecialtiesAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<SpecialtyDto>>(
            $"/api/specialties?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateSpecialtyAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/specialties", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSpecialtyAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/specialties/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSpecialtyAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/specialties/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Contractors ────────────────────────────────────────────────────────────
    public async Task<List<ContractorDto>> GetContractorsAsync(int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiListResponse<ContractorDto>>(
            $"/api/contractors?page={page}&pageSize={pageSize}");
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateContractorAsync(object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync("/api/contractors", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateContractorAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/contractors/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteContractorAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/contractors/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Documents ──────────────────────────────────────────────────────────────
    public async Task<List<DocumentDto>> GetDocumentsAsync(Guid? projectId = null, string? category = null, int page = 1, int pageSize = 100)
    {
        await AddAuthHeaderAsync();
        var url = $"/api/documents?page={page}&pageSize={pageSize}";
        if (projectId.HasValue) url += $"&projectId={projectId.Value}";
        if (!string.IsNullOrEmpty(category)) url += $"&category={Uri.EscapeDataString(category)}";
        var resp = await _http.GetFromJsonAsync<ApiListResponse<DocumentDto>>(url);
        return resp?.Data ?? [];
    }

    public async Task<bool> UploadDocumentAsync(Guid projectId, string name, string category, string? description, string? version, MultipartFormDataContent content)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsync("/api/documents", content);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateDocumentAsync(Guid id, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/documents/{id}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/documents/{id}");
        return r.IsSuccessStatusCode;
    }

    public async Task<byte[]?> DownloadDocumentAsync(Guid id)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetAsync($"/api/documents/{id}/download");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync();
    }

    // ── Project Stages ──────────────────────────────────────────────────────────
    public async Task<List<StageDto>> GetProjectStagesAsync(Guid projectId)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<StageDto>>>($"/api/projects/{projectId}/stages");
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateProjectStageAsync(Guid projectId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/projects/{projectId}/stages", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProjectStageAsync(Guid projectId, Guid stageId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/projects/{projectId}/stages/{stageId}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectStageAsync(Guid projectId, Guid stageId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/projects/{projectId}/stages/{stageId}");
        return r.IsSuccessStatusCode;
    }

    // ── Project Sectors ─────────────────────────────────────────────────────────
    public async Task<List<SectorDto>> GetProjectSectorsAsync(Guid projectId)
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<SectorDto>>>($"/api/projects/{projectId}/sectors");
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateProjectSectorAsync(Guid projectId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/projects/{projectId}/sectors", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProjectSectorAsync(Guid projectId, Guid sectorId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/projects/{projectId}/sectors/{sectorId}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectSectorAsync(Guid projectId, Guid sectorId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/projects/{projectId}/sectors/{sectorId}");
        return r.IsSuccessStatusCode;
    }

    // ── Project Units ───────────────────────────────────────────────────────────
    public async Task<List<UnitDto>> GetProjectUnitsAsync(Guid projectId, Guid? sectorId = null)
    {
        await AddAuthHeaderAsync();
        var url = $"/api/projects/{projectId}/units";
        if (sectorId.HasValue) url += $"?sectorId={sectorId.Value}";
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<UnitDto>>>(url);
        return resp?.Data ?? [];
    }

    public async Task<bool> CreateProjectUnitAsync(Guid projectId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PostAsJsonAsync($"/api/projects/{projectId}/units", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProjectUnitAsync(Guid projectId, Guid unitId, object payload)
    {
        await AddAuthHeaderAsync();
        var r = await _http.PutAsJsonAsync($"/api/projects/{projectId}/units/{unitId}", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectUnitAsync(Guid projectId, Guid unitId)
    {
        await AddAuthHeaderAsync();
        var r = await _http.DeleteAsync($"/api/projects/{projectId}/units/{unitId}");
        return r.IsSuccessStatusCode;
    }
}
