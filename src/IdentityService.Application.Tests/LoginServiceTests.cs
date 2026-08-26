using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Tests;

public sealed class LoginServiceTests
{
    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAuthResponseAndPersistsRefreshToken()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");
        user.SetPasswordHash("hashed::password123");

        var userRepository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var tokenGenerator = new FakeJwtTokenGenerator();
        var service = new LoginService(
            userRepository,
            unitOfWork,
            new FakePasswordHasher(),
            new PassThroughValidator<LoginRequest>(),
            new AuthResponseFactory(tokenGenerator),
            tokenGenerator);

        var response = await service.LoginAsync(new LoginRequest("user@example.com", "password123"));

        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(user.RefreshTokens, refreshToken => refreshToken.Token == "refresh-token");
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(tokenGenerator.ExpiresAtUtc, response.AccessTokenExpiresAtUtc);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ThrowsAuthenticationException()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");
        user.SetPasswordHash("hashed::different");

        var unitOfWork = new FakeUnitOfWork();
        var service = new LoginService(
            new FakeUserRepository(user),
            unitOfWork,
            new FakePasswordHasher(),
            new PassThroughValidator<LoginRequest>(),
            new AuthResponseFactory(new FakeJwtTokenGenerator()),
            new FakeJwtTokenGenerator());

        var action = () => service.LoginAsync(new LoginRequest("user@example.com", "password123"));

        var exception = await Assert.ThrowsAsync<AuthenticationException>(action);
        Assert.Equal("Invalid credentials.", exception.Message);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeUserRepository(User? userByEmail) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(userByEmail);
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