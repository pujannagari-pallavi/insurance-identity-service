using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Tests;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenRequestIsValid_CreatesUserAndPersistsChanges()
    {
        var userRepository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var passwordHasher = new FakePasswordHasher();
        var defaultRoleAssigner = new FakeDefaultRoleAssigner();
        var validator = new PassThroughValidator<RegisterRequest>();
        var tokenGenerator = new FakeJwtTokenGenerator();
        var service = new RegistrationService(
            userRepository,
            unitOfWork,
            passwordHasher,
            defaultRoleAssigner,
            validator,
            new AuthResponseFactory(tokenGenerator),
            tokenGenerator);

        var response = await service.RegisterAsync(new RegisterRequest(" user@example.com ", " new-user ", "password123"));

        Assert.NotNull(userRepository.AddedUser);
        Assert.Equal("user@example.com", userRepository.AddedUser!.Email);
        Assert.Equal("new-user", userRepository.AddedUser.UserName);
        Assert.Equal("hashed::password123", userRepository.AddedUser.PasswordHash);
        Assert.Contains(userRepository.AddedUser.Roles, role => role.Name == DefaultRoles.Customer);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(tokenGenerator.ExpiresAtUtc, response.AccessTokenExpiresAtUtc);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsValidationException()
    {
        var existingUser = new User(Guid.NewGuid(), "user@example.com", "existing-user");
        var userRepository = new FakeUserRepository { UserByEmail = existingUser };
        var unitOfWork = new FakeUnitOfWork();
        var service = new RegistrationService(
            userRepository,
            unitOfWork,
            new FakePasswordHasher(),
            new FakeDefaultRoleAssigner(),
            new PassThroughValidator<RegisterRequest>(),
            new AuthResponseFactory(new FakeJwtTokenGenerator()),
            new FakeJwtTokenGenerator());

        var action = () => service.RegisterAsync(new RegisterRequest("user@example.com", "new-user", "password123"));

        var exception = await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Equal("A user with this email already exists.", exception.Message);
        Assert.Null(userRepository.AddedUser);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? UserByEmail { get; init; }

        public User? AddedUser { get; private set; }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserByEmail);
        }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            AddedUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"hashed::{password}";
        }

        public bool Verify(string password, string passwordHash)
        {
            return passwordHash == Hash(password);
        }
    }

    private sealed class FakeDefaultRoleAssigner : IDefaultRoleAssigner
    {
        public Task AssignAsync(User user, CancellationToken cancellationToken = default)
        {
            user.AssignRole(new Role(Guid.NewGuid(), DefaultRoles.Customer, "Default customer role"));
            return Task.CompletedTask;
        }
    }

    private sealed class PassThroughValidator<T> : IValidator<T>
    {
        public void Validate(T value)
        {
        }
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public DateTime ExpiresAtUtc { get; } = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public AccessTokenResult GenerateAccessToken(User user)
        {
            return new AccessTokenResult("access-token", ExpiresAtUtc);
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken("refresh-token", DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        }
    }
}