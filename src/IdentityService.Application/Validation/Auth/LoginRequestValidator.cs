using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;

namespace IdentityService.Application.Validation.Auth;

public sealed class LoginRequestValidator : IValidator<LoginRequest>
{
    public void Validate(LoginRequest value)
    {
        if (string.IsNullOrWhiteSpace(value.Email) || string.IsNullOrWhiteSpace(value.Password))
        {
            throw new ValidationException("Email and password are required.");
        }
    }
}