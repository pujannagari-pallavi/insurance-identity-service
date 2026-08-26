using IdentityService.Domain.Entities;

namespace IdentityService.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    AccessTokenResult GenerateAccessToken(User user);

    RefreshToken GenerateRefreshToken();
}