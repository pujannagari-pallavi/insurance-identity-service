using System.Net;
using System.Net.Http.Json;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Tests;

public sealed class AuthControllerIntegrationTests
{
    [Fact]
    public async Task Register_WhenServiceThrowsValidationException_ReturnsBadRequestProblemDetails()
    {
        using var factory = new IdentityApiFactory(
            new ThrowingRegistrationService(new ValidationException("Email is required.")),
            new StubLoginService(),
            new StubRefreshTokenService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("", "user", "password123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Validation failed.", problem.Title);
        Assert.Equal("Email is required.", problem.Detail);
    }

    [Fact]
    public async Task Login_WhenServiceThrowsAuthenticationException_ReturnsUnauthorizedProblemDetails()
    {
        using var factory = new IdentityApiFactory(
            new StubRegistrationService(),
            new ThrowingLoginService(new AuthenticationException("Invalid credentials.")),
            new StubRefreshTokenService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("user@example.com", "bad-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(401, problem!.Status);
        Assert.Equal("Authentication failed.", problem.Title);
        Assert.Equal("Invalid credentials.", problem.Detail);
    }

    [Fact]
    public async Task Health_ReturnsHealthyResponse()
    {
        using var factory = new IdentityApiFactory(
            new StubRegistrationService(),
            new StubLoginService(),
            new StubRefreshTokenService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class ThrowingRegistrationService(Exception exception) : IRegistrationService
    {
        public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromException<AuthResponse>(exception);
        }
    }

    private sealed class ThrowingLoginService(Exception exception) : ILoginService
    {
        public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromException<AuthResponse>(exception);
        }
    }

    private sealed class StubRegistrationService : IRegistrationService
    {
        public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuthResponse(Guid.NewGuid(), request.Email, request.UserName, "token", "refresh", DateTime.UtcNow.AddMinutes(30), Array.Empty<string>(), Array.Empty<string>()));
        }
    }

    private sealed class StubLoginService : ILoginService
    {
        public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuthResponse(Guid.NewGuid(), request.Email, "user", "token", "refresh", DateTime.UtcNow.AddMinutes(30), Array.Empty<string>(), Array.Empty<string>()));
        }
    }

    private sealed class StubRefreshTokenService : IRefreshTokenService
    {
        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AuthResponse(Guid.NewGuid(), "user@example.com", "user", "token", request.RefreshToken, DateTime.UtcNow.AddMinutes(30), Array.Empty<string>(), Array.Empty<string>()));
        }
    }
}