using IdentityService.Application.Abstractions.Authentication;
using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Tests;

public sealed class RefreshTokenServiceTests
{
    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsActive_RevokesCurrentTokenAndReturnsNewTokens()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");
        var currentToken = new RefreshToken("current-refresh-token", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(currentToken);

        var userRepository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var tokenGenerator = new FakeJwtTokenGenerator();
        var service = new RefreshTokenService(
            userRepository,
            unitOfWork,
            new PassThroughValidator<RefreshTokenRequest>(),
            new AuthResponseFactory(tokenGenerator),
            tokenGenerator);

        var response = await service.RefreshTokenAsync(new RefreshTokenRequest("current-refresh-token"));

        Assert.True(currentToken.IsRevoked);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Contains(user.RefreshTokens, refreshToken => refreshToken.Token == "new-refresh-token");
        Assert.Equal("new-access-token", response.AccessToken);
        Assert.Equal("new-refresh-token", response.RefreshToken);
        Assert.Equal(tokenGenerator.ExpiresAtUtc, response.AccessTokenExpiresAtUtc);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpired_ThrowsAuthenticationException()
    {
        var user = new User(Guid.NewGuid(), "user@example.com", "user-name");
        user.AddRefreshToken(new RefreshToken("expired-refresh-token", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddMinutes(-1)));

        var unitOfWork = new FakeUnitOfWork();
        var service = new RefreshTokenService(
            new FakeUserRepository(user),
            unitOfWork,
            new PassThroughValidator<RefreshTokenRequest>(),
            new AuthResponseFactory(new FakeJwtTokenGenerator()),
            new FakeJwtTokenGenerator());

        var action = () => service.RefreshTokenAsync(new RefreshTokenRequest("expired-refresh-token"));

        var exception = await Assert.ThrowsAsync<AuthenticationException>(action);
        Assert.Equal("Refresh token is expired or revoked.", exception.Message);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeUserRepository(User? userByRefreshToken) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(userByRefreshToken);
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

    private sealed class PassThroughValidator<T> : IValidator<T>
    {
        public void Validate(T value)
        {
        }
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public DateTime ExpiresAtUtc { get; } = new(2030, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        public AccessTokenResult GenerateAccessToken(User user)
        {
            return new AccessTokenResult("new-access-token", ExpiresAtUtc);
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken("new-refresh-token", DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        }
    }
}