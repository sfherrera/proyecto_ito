namespace ITO.Cloud.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id, string FirstName, string LastName, string FullName,
    string Email, string? Rut, string? Position, string? AvatarUrl,
    bool IsActive, Guid TenantId, DateTime CreatedAt, IList<string> Roles);

public record CreateUserDto(
    string FirstName, string LastName, string Email, string Password,
    string? Rut = null, string? Position = null,
    IList<string>? Roles = null);

public record UpdateUserDto(
    string FirstName, string LastName, string? Rut,
    string? Position, bool IsActive);
