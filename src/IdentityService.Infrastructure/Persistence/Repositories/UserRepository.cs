using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();

        return dbContext.Users
            .Include(user => user.Roles)
                .ThenInclude(role => role.Permissions)
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(
                user => user.Email == normalizedEmail,
                cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.Roles)
                .ThenInclude(role => role.Permissions)
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .Include(user => user.Roles)
                .ThenInclude(role => role.Permissions)
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(
                user => user.RefreshTokens.Any(token => token.Token == refreshToken),
                cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }
}