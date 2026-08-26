using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Tests;

public sealed class DefaultRoleAssignerTests
{
    [Fact]
    public async Task AssignAsync_WhenDefaultRoleExists_AssignsRoleToUser()
    {
        var role = new Role(Guid.NewGuid(), DefaultRoles.Customer, "Default customer role");
        var assigner = new DefaultRoleAssigner(new FakeRoleRepository(role));
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");

        await assigner.AssignAsync(user);

        Assert.Contains(user.Roles, existingRole => existingRole.Name == DefaultRoles.Customer);
    }

    [Fact]
    public async Task AssignAsync_WhenDefaultRoleDoesNotExist_ThrowsInvalidOperationException()
    {
        var assigner = new DefaultRoleAssigner(new FakeRoleRepository(null));
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");

        var action = () => assigner.AssignAsync(user);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal("Default Customer role is not configured.", exception.Message);
    }

    private sealed class FakeRoleRepository(Role? role) : IRoleRepository
    {
        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(role);
        }
    }
}