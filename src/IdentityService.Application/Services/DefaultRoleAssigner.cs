using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Services;

public sealed class DefaultRoleAssigner(IRoleRepository roleRepository) : IDefaultRoleAssigner
{
    public async Task AssignAsync(User user, CancellationToken cancellationToken = default)
    {
        var defaultRole = await roleRepository.GetByNameAsync(DefaultRoles.Customer, cancellationToken)
            ?? throw new InvalidOperationException("Default Customer role is not configured.");

        user.AssignRole(defaultRole);
    }
}