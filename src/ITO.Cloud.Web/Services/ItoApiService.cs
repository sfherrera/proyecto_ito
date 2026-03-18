using ITO.Cloud.Web.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ITO.Cloud.Web.Services;

// ── DTOs simples para el frontend ────────────────────────────────────────────
public record CompanyDto(Guid Id, string Name, string? Rut, string CompanyType, bool IsActive);
public record ProjectDto(Guid Id, string Code, string Name, string? Status, string? CompanyName, DateTime? StartDate);
public record InspectionDto(Guid Id, string Code, string Title, string Status, string? AssignedToName, DateTime? ScheduledDate, decimal? Score);
public record ObservationDto(Guid Id, string Code, string Title, string Status, string Severity, DateOnly? DueDate, bool IsOverdue);
public record DashboardDto(int TotalInspections, int InspectionsThisMonth, int OpenObservations, int CriticalObservations, int OverdueObservations, double ComplianceRate);
public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class ItoApiService
{
    private readonly HttpClient _http;
    private readonly TokenStorageService _tokenStorage;

    public ItoApiService(HttpClient http, TokenStorageService tokenStorage)
    {
        _http         = http;
        _tokenStorage = tokenStorage;
    }

    private void AddAuthHeader()
    {
        var token = _tokenStorage.GetToken();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    public async Task<DashboardDto?> GetDashboardAsync()
    {
        AddAuthHeader();
        var resp = await _http.GetFromJsonAsync<ApiResponse<DashboardDto>>("/api/dashboard");
        return resp?.Data;
    }

    // ── Companies ─────────────────────────────────────────────────────────────
    public async Task<List<CompanyDto>> GetCompaniesAsync(int page = 1, int pageSize = 50)
    {
        AddAuthHeader();
        var resp = await _http.GetFromJsonAsync<ApiResponse<PaginatedResult<CompanyDto>>>(
            $"/api/companies?page={page}&pageSize={pageSize}");
        return resp?.Data?.Items ?? [];
    }

    public async Task<bool> CreateCompanyAsync(object payload)
    {
        AddAuthHeader();
        var r = await _http.PostAsJsonAsync("/api/companies", payload);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCompanyAsync(Guid id)
    {
        AddAuthHeader();
        var r = await _http.DeleteAsync($"/api/companies/{id}");
        return r.IsSuccessStatusCode;
    }

    // ── Projects ──────────────────────────────────────────────────────────────
    public async Task<List<ProjectDto>> GetProjectsAsync(int page = 1, int pageSize = 50)
    {
        AddAuthHeader();
        var resp = await _http.GetFromJsonAsync<ApiResponse<PaginatedResult<ProjectDto>>>(
            $"/api/projects?page={page}&pageSize={pageSize}");
        return resp?.Data?.Items ?? [];
    }

    // ── Inspections ───────────────────────────────────────────────────────────
    public async Task<List<InspectionDto>> GetInspectionsAsync(int page = 1, int pageSize = 50)
    {
        AddAuthHeader();
        var resp = await _http.GetFromJsonAsync<ApiResponse<PaginatedResult<InspectionDto>>>(
            $"/api/inspections?page={page}&pageSize={pageSize}");
        return resp?.Data?.Items ?? [];
    }

    // ── Observations ──────────────────────────────────────────────────────────
    public async Task<List<ObservationDto>> GetObservationsAsync(int page = 1, int pageSize = 50)
    {
        AddAuthHeader();
        var resp = await _http.GetFromJsonAsync<ApiResponse<PaginatedResult<ObservationDto>>>(
            $"/api/observations?page={page}&pageSize={pageSize}");
        return resp?.Data?.Items ?? [];
    }
}

// Wrapper de la respuesta estándar de la API
public record ApiResponse<T>(bool Success, T? Data, string? Message);
