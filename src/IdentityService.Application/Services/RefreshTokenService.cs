using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public sealed class RefreshTokenService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IValidator<RefreshTokenRequest> refreshTokenRequestValidator,
    AuthResponseFactory authResponseFactory,
    IJwtTokenGenerator jwtTokenGenerator) : IRefreshTokenService
{
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        refreshTokenRequestValidator.Validate(request);

        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new AuthenticationException("Invalid refresh token.");

        var existingToken = user.GetRefreshToken(request.RefreshToken);
        if (existingToken is null || !existingToken.IsActive(DateTime.UtcNow))
        {
            throw new AuthenticationException("Refresh token is expired or revoked.");
        }

        existingToken.Revoke(DateTime.UtcNow);

        var newRefreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(newRefreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return authResponseFactory.Create(user, newRefreshToken);
    }
}