using System.Security.Claims;
using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Services;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IRegistrationService registrationService,
    ILoginService loginService,
    IRefreshTokenService refreshTokenService,
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IPasswordResetEmailService passwordResetEmailService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await registrationService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await loginService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await refreshTokenService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) return Ok();

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive) return Ok();

        var nowUtc = DateTime.UtcNow;
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(item => item.UserId == user.Id && item.UsedAtUtc == null && item.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
        foreach (var token in existingTokens) token.Use(nowUtc);

        var resetToken = PasswordResetTokenFactory.Create(user.Id);
        dbContext.PasswordResetTokens.Add(resetToken.Token);
        await dbContext.SaveChangesAsync(cancellationToken);
        await passwordResetEmailService.SendAsync(user.Email, user.UserName, resetToken.RawToken, false, cancellationToken);
        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return BadRequest(new { detail = "A valid reset token and a password of at least 8 characters are required." });
        }

        var nowUtc = DateTime.UtcNow;
        var tokenHash = PasswordResetTokenFactory.Hash(request.Token);
        var resetToken = await dbContext.PasswordResetTokens.SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);
        if (resetToken is null || !resetToken.IsUsable(nowUtc)) return BadRequest(new { detail = "This password link is invalid or has expired." });

        var user = await dbContext.Users.Include(item => item.RefreshTokens).SingleOrDefaultAsync(item => item.Id == resetToken.UserId, cancellationToken);
        if (user is null || !user.IsActive) return BadRequest(new { detail = "This password link is invalid or has expired." });

        user.SetPasswordHash(passwordHasher.Hash(request.Password));
        user.RevokeRefreshTokens(nowUtc);
        resetToken.Use(nowUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var permissions = User.Claims
            .Where(claim => claim.Type == "permission")
            .Select(claim => claim.Value)
            .ToArray();

        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            userName = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray(),
            permissions
        });
    }
}

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string Password);