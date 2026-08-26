using IdentityService.Application.Contracts.Auth;

namespace IdentityService.Application.Services;

public interface ILoginService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}