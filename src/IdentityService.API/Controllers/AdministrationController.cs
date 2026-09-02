using IdentityService.Domain.Entities;
using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Services;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/administration")]
public sealed class AdministrationController(IdentityDbContext dbContext, IPasswordResetEmailService passwordResetEmailService) : ControllerBase
{
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateOperationalUserRequest request, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest(new { detail = "Email and username are required." });
        }

        if (await dbContext.Users.AnyAsync(user => user.Email == request.Email.Trim(), cancellationToken))
        {
            return BadRequest(new { detail = "A user with this email already exists." });
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await dbContext.Roles.Where(role => roleIds.Contains(role.Id)).ToListAsync(cancellationToken);
        if (roles.Count != roleIds.Length || roles.Count == 0)
        {
            return BadRequest(new { detail = "Assign at least one valid operational role." });
        }

        if (roles.Any(role => role.Name == DefaultRoles.Customer))
        {
            return BadRequest(new { detail = "Customer accounts must be created through customer registration." });
        }

        var user = new User(Guid.NewGuid(), request.Email.Trim(), request.UserName.Trim());
        user.ReplaceRoles(roles);
        dbContext.Users.Add(user);
        var resetToken = PasswordResetTokenFactory.Create(user.Id);
        dbContext.PasswordResetTokens.Add(resetToken.Token);
        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), user.Id, "OperationalUserCreated", string.Join(",", roles.Select(role => role.Name))));
        await dbContext.SaveChangesAsync(cancellationToken);
        await passwordResetEmailService.SendAsync(user.Email, user.UserName, resetToken.RawToken, true, cancellationToken);

        return Ok(new { user.Id, user.Email, user.UserName, user.IsActive, Roles = user.Roles.Select(role => role.Name) });
    }

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

        var removesPlatformAdmin = user.Roles.Any(role => role.Name == DefaultRoles.PlatformAdmin)
            && roles.All(role => role.Name != DefaultRoles.PlatformAdmin);
        if (removesPlatformAdmin && await dbContext.Users.CountAsync(item => item.Roles.Any(role => role.Name == DefaultRoles.PlatformAdmin), cancellationToken) <= 1)
        {
            return BadRequest(new { detail = "The last PlatformAdmin role cannot be removed." });
        }

        user.ReplaceRoles(roles);
        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), userId, "RolesUpdated", string.Join(",", roles.Select(role => role.Name))));
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var user = await dbContext.Users.Include(item => item.RefreshTokens).SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return NotFound();
        if (userId == ActorId() && !request.IsActive) return BadRequest(new { detail = "You cannot deactivate your own account." });

        user.SetActive(request.IsActive);
        if (!request.IsActive) user.RevokeRefreshTokens(DateTime.UtcNow);
        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), userId, request.IsActive ? "UserActivated" : "UserDeactivated", user.Email));
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/revoke-sessions")]
    public async Task<IActionResult> RevokeSessions(Guid userId, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var user = await dbContext.Users.Include(item => item.RefreshTokens).SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return NotFound();
        user.RevokeRefreshTokens(DateTime.UtcNow);
        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), userId, "SessionsRevoked", user.Email));
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/send-password-reset")]
    public async Task<IActionResult> SendPasswordReset(Guid userId, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return NotFound();

        var nowUtc = DateTime.UtcNow;
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(item => item.UserId == user.Id && item.UsedAtUtc == null && item.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
        foreach (var token in existingTokens) token.Use(nowUtc);
        var resetToken = PasswordResetTokenFactory.Create(user.Id);
        dbContext.PasswordResetTokens.Add(resetToken.Token);
        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), userId, "PasswordResetSent", user.Email));
        await dbContext.SaveChangesAsync(cancellationToken);
        await passwordResetEmailService.SendAsync(user.Email, user.UserName, resetToken.RawToken, false, cancellationToken);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        RequireUserManagement();
        if (userId == ActorId()) return BadRequest(new { detail = "You cannot delete your own account." });

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null) return NotFound();

        dbContext.AdministrationAuditEntries.Add(new AdministrationAuditEntry(ActorId(), userId, "UserDeleted", user.Email));
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAudit(CancellationToken cancellationToken)
    {
        RequireUserManagement();
        var entries = await dbContext.AdministrationAuditEntries.AsNoTracking().OrderByDescending(entry => entry.OccurredAtUtc).Take(100)
            .Select(entry => new { entry.Id, entry.ActorUserId, entry.TargetUserId, entry.Action, entry.Details, entry.OccurredAtUtc }).ToListAsync(cancellationToken);
        return Ok(entries);
    }

    private void RequireUserManagement()
    {
        if (!User.HasClaim("permission", "Identity.Users.Manage"))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage users.");
        }
    }

    private Guid ActorId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
        ? userId : throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
}

public sealed record UpdateUserRolesRequest(IReadOnlyCollection<Guid> RoleIds);
public sealed record UpdateUserStatusRequest(bool IsActive);
public sealed record CreateOperationalUserRequest(string Email, string UserName, IReadOnlyCollection<Guid> RoleIds);

