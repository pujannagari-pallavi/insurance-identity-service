namespace IdentityService.Application.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password);