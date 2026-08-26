using IdentityService.Application.Contracts.Auth;

namespace IdentityService.Application.Services;

public interface IRegistrationService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}