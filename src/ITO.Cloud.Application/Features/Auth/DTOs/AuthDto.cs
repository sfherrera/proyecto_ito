namespace ITO.Cloud.Application.Features.Auth.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string AccessToken, DateTime ExpiresAt,
    Guid UserId, string FullName, string Email,
    Guid TenantId, string TenantName,
    IList<string> Roles);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmPassword);
