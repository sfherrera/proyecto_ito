using ITO.Cloud.Application.Features.Auth.DTOs;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Infrastructure.Identity;
using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtService;
    private readonly ApplicationDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtService,
        ApplicationDbContext db)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _jwtService    = jwtService;
        _db            = db;
    }

    /// <summary>Login — devuelve JWT</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive || user.IsDeleted)
            return Unauthorized(new { success = false, message = "Credenciales inválidas." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new { success = false, message = "Cuenta bloqueada temporalmente." });
            return Unauthorized(new { success = false, message = "Credenciales inválidas." });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var (token, expiresAt) = await _jwtService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);

        return Ok(new
        {
            success = true,
            data = new LoginResponse(
                AccessToken: token,
                ExpiresAt:   expiresAt,
                UserId:      user.Id,
                FullName:    user.FullName,
                Email:       user.Email!,
                TenantId:    user.TenantId,
                TenantName:  tenant?.Name ?? string.Empty,
                Roles:       roles.ToList())
        });
    }

    /// <summary>Datos del usuario autenticado</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var user  = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new { success = true, data = new
        {
            user.Id, user.FirstName, user.LastName, user.FullName,
            user.Email, user.Rut, user.Position, user.AvatarUrl,
            user.TenantId, Roles = roles
        }});
    }

    /// <summary>Cambiar contraseña</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user   = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { success = false, message = "Las contraseñas no coinciden." });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });

        return Ok(new { success = true, message = "Contraseña actualizada." });
    }
}
