namespace ITO.Cloud.Application.Features.Companies.DTOs;

public record CompanyDto(
    Guid Id, string Name, string? Rut, string? BusinessName, string CompanyType,
    string? Address, string? City, string? Region, string? Phone, string? Email,
    string? Website, string? LogoUrl, bool IsActive, string? Notes,
    DateTime CreatedAt);

public record CreateCompanyDto(
    string Name, string? Rut, string? BusinessName,
    string CompanyType = "constructora",
    string? Address = null, string? City = null, string? Region = null,
    string? Phone = null, string? Email = null, string? Website = null,
    string? Notes = null);

public record UpdateCompanyDto(
    string Name, string? Rut, string? BusinessName,
    string CompanyType, string? Address, string? City, string? Region,
    string? Phone, string? Email, string? Website, bool IsActive, string? Notes);

public record TenantDto(Guid Id, string Name, string Slug, string Plan, bool IsActive);
