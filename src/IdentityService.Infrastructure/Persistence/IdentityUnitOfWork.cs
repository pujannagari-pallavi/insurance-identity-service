using IdentityService.Domain.Repositories;

namespace IdentityService.Infrastructure.Persistence;

public sealed class IdentityUnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}