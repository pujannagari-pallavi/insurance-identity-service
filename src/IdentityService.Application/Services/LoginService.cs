using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public sealed class LoginService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IValidator<LoginRequest> loginRequestValidator,
    AuthResponseFactory authResponseFactory,
    IJwtTokenGenerator jwtTokenGenerator) : ILoginService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        loginRequestValidator.Validate(request);

        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthenticationException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid credentials.");
        }

        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return authResponseFactory.Create(user, refreshToken);
    }
}