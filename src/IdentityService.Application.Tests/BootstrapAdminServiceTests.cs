using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Tests;

public sealed class BootstrapAdminServiceTests
{
    [Fact]
    public async Task EnsureCreatedAsync_WhenUserDoesNotExist_CreatesPlatformAdmin()
    {
        var userRepository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new BootstrapAdminService(
            userRepository,
            new FakeRoleRepository(new Role(Guid.NewGuid(), DefaultRoles.PlatformAdmin, "Platform administrator")),
            unitOfWork,
            new FakePasswordHasher());

        var wasCreated = await service.EnsureCreatedAsync(" admin@example.com ", " Platform Admin ", "strong-password");

        Assert.True(wasCreated);
        Assert.NotNull(userRepository.AddedUser);
        Assert.Equal("admin@example.com", userRepository.AddedUser!.Email);
        Assert.Equal("Platform Admin", userRepository.AddedUser.UserName);
        Assert.Equal("hashed::strong-password", userRepository.AddedUser.PasswordHash);
        Assert.Contains(userRepository.AddedUser.Roles, role => role.Name == DefaultRoles.PlatformAdmin);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task EnsureCreatedAsync_WhenUserExists_DoesNotModifyIt()
    {
        var userRepository = new FakeUserRepository
        {
            ExistingUser = new User(Guid.NewGuid(), "admin@example.com", "Existing user")
        };
        var service = new BootstrapAdminService(
            userRepository,
            new FakeRoleRepository(null),
            new FakeUnitOfWork(),
            new FakePasswordHasher());

        var wasCreated = await service.EnsureCreatedAsync("admin@example.com", "Platform Admin", "strong-password");

        Assert.False(wasCreated);
        Assert.Null(userRepository.AddedUser);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? ExistingUser { get; init; }
        public User? AddedUser { get; private set; }
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(ExistingUser);
        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task AddAsync(User user, CancellationToken cancellationToken = default) { AddedUser = user; return Task.CompletedTask; }
    }

    private sealed class FakeRoleRepository(Role? role) : IRoleRepository
    {
        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(role);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) { SaveChangesCallCount++; return Task.CompletedTask; }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed::{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}