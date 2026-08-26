using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}