namespace IdentityService.Application.Contracts.Auth;

public sealed record RegisterRequest(string Email, string UserName, string Password);