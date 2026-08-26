using IdentityService.Application.Contracts.Auth;

namespace IdentityService.Application.Services;

public interface IRefreshTokenService
{
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}