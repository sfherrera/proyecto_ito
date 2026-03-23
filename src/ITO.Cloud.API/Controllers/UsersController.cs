using ITO.Cloud.Application.Features.Users.DTOs;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de usuarios del tenant</summary>
public class UsersController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db          = db;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search) ||
                                     u.FirstName.Contains(search) ||
                                     u.LastName.Contains(search));
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        var dtos = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserDto(u.Id, u.FirstName, u.LastName, u.FullName,
                u.Email!, u.Rut, u.Position, u.AvatarUrl, u.IsActive,
                u.TenantId, u.CreatedAt, roles.ToList()));
        }

        return OkPaginated(Application.Common.Models.PaginatedList<UserDto>.Create(dtos, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Management)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(new { success = false, message = "Usuario no encontrado." });
        var roles = await _userManager.GetRolesAsync(user);
        return OkData(new UserDto(user.Id, user.FirstName, user.LastName, user.FullName,
            user.Email!, user.Rut, user.Position, user.AvatarUrl, user.IsActive,
            user.TenantId, user.CreatedAt, roles.ToList()));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto,
        [FromServices] Domain.Interfaces.ITenantContext tenantContext)
    {
        var user = new ApplicationUser
        {
            TenantId  = tenantContext.TenantId,
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            UserName  = dto.Email,
            Email     = dto.Email,
            Rut       = dto.Rut,
            Position  = dto.Position
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });

        if (dto.Roles?.Count > 0)
            await _userManager.AddToRolesAsync(user, dto.Roles);

        return CreatedData(new { user.Id, user.Email, user.FullName });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admins)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName  = dto.LastName;
        user.Rut       = dto.Rut;
        user.Position  = dto.Position;
        user.IsActive  = dto.IsActive;

        await _userManager.UpdateAsync(user);
        return Ok(new { success = true, message = "Usuario actualizado." });
    }
}
