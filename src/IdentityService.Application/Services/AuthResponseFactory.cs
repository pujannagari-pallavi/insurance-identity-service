using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public sealed class AuthResponseFactory(IJwtTokenGenerator jwtTokenGenerator)
{
    public AuthResponse Create(User user, RefreshToken refreshToken)
    {
        var roles = user.Roles.Select(role => role.Name).ToArray();
        var permissions = user.GetPermissions().ToArray();
        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAtUtc,
            roles,
            permissions);
    }
}