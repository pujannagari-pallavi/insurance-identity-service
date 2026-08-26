using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;

namespace IdentityService.Application.Validation.Auth;

public sealed class RefreshTokenRequestValidator : IValidator<RefreshTokenRequest>
{
    public void Validate(RefreshTokenRequest value)
    {
        if (string.IsNullOrWhiteSpace(value.RefreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }
    }
}