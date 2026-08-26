namespace IdentityService.Application.Contracts.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string UserName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);