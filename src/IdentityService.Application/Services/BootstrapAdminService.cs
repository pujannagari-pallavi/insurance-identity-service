using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public sealed class BootstrapAdminService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
{
    public async Task<bool> EnsureCreatedAsync(
        string email,
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            return false;
        }

        var platformAdminRole = await roleRepository.GetByNameAsync(DefaultRoles.PlatformAdmin, cancellationToken)
            ?? throw new InvalidOperationException("The PlatformAdmin role was not found after migrations completed.");

        var user = new User(Guid.NewGuid(), normalizedEmail, userName.Trim());
        user.SetPasswordHash(passwordHasher.Hash(password));
        user.AssignRole(platformAdminRole);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}