using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(IdentityDbContext dbContext) : IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return dbContext.Roles
            .Include(role => role.Permissions)
            .FirstOrDefaultAsync(
                role => role.Name == normalizedName,
                cancellationToken);
    }
}