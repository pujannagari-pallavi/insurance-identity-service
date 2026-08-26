using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public sealed class RegistrationService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IDefaultRoleAssigner defaultRoleAssigner,
    IValidator<RegisterRequest> registerRequestValidator,
    AuthResponseFactory authResponseFactory,
    IJwtTokenGenerator jwtTokenGenerator) : IRegistrationService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        registerRequestValidator.Validate(request);

        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ValidationException("A user with this email already exists.");
        }

        var user = new User(Guid.NewGuid(), request.Email.Trim(), request.UserName.Trim());
        user.SetPasswordHash(passwordHasher.Hash(request.Password));

        await defaultRoleAssigner.AssignAsync(user, cancellationToken);

        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.AddRefreshToken(refreshToken);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return authResponseFactory.Create(user, refreshToken);
    }
}