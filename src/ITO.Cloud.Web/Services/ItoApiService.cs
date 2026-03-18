using ITO.Cloud.Web.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ITO.Cloud.Web.Services;

// ── DTOs de lista (vistas resumidas) ─────────────────────────────────────────
public record CompanyDto(Guid Id, string Name, string? Rut, string CompanyType, bool IsActive);
public record ProjectDto(Guid Id, string Code, string Name, string? Status, string? CompanyName, string? StartDate);
public record InspectionDto(Guid Id, string Code, string Title, string Status, string? AssignedToName, DateTime? ScheduledDate, decimal? Score);
public record ObservationDto(Guid Id, string Code, string Title, string Status, string Severity, string? DueDate, bool IsOverdue);
public record DashboardDto(int TotalInspections, int InspectionsThisMonth, int OpenObservations, int CriticalObservations, int OverdueObservations, double ComplianceRate);
public record TemplateItemDto(Guid Id, string Name, string? TemplateType, string Status);
public record UserDto(Guid Id, string FirstName, string LastName, string FullName, string Email, string? Rut, string? Position, bool IsActive, DateTime CreatedAt, List<string>? Roles);

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

    // ── Templates ─────────────────────────────────────────────────────────────
    public async Task<List<TemplateItemDto>> GetTemplatesAsync()
    {
        await AddAuthHeaderAsync();
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<TemplateItemDto>>>("/api/templates");
        return resp?.Data ?? [];
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
}
