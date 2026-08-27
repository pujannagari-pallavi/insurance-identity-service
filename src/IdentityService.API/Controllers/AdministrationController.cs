using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/administration")]
public sealed class AdministrationController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        RequireUserManagement();
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var usersQuery = dbContext.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            usersQuery = usersQuery.Where(user =>
                user.Email.ToLower().Contains(searchTerm) || user.UserName.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleName = role.Trim();
            usersQuery = usersQuery.Where(user => user.Roles.Any(item => item.Name == roleName));
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery.OrderBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new { user.Id, user.Email, user.UserName, user.IsActive, Roles = user.Roles.Select(role => role.Name) })
            .ToListAsync(cancellationToken);
        return Ok(new { items = users, totalCount, page, pageSize });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var roles = await dbContext.Roles.OrderBy(role => role.Name)
            .Select(role => new { role.Id, role.Name, role.Description })
            .ToListAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPut("users/{userId:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var user = await dbContext.Users.Include(item => item.Roles)
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return NotFound();

        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await dbContext.Roles.Where(role => roleIds.Contains(role.Id)).ToListAsync(cancellationToken);
        if (roles.Count != roleIds.Length) return BadRequest(new { detail = "One or more role IDs do not exist." });

        user.ReplaceRoles(roles);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private void RequireUserManagement()
    {
        if (!User.HasClaim("permission", "Identity.Users.Manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage users.");
        }
    }
}

public sealed record UpdateUserRolesRequest(IReadOnlyCollection<Guid> RoleIds);

