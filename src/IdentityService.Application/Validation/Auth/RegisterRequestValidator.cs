using IdentityService.Application.Abstractions.Validation;
using IdentityService.Application.Contracts.Auth;
using IdentityService.Application.Exceptions;

namespace IdentityService.Application.Validation.Auth;

public sealed class RegisterRequestValidator : IValidator<RegisterRequest>
{
    public void Validate(RegisterRequest value)
    {
        if (string.IsNullOrWhiteSpace(value.Email) ||
            string.IsNullOrWhiteSpace(value.UserName) ||
            string.IsNullOrWhiteSpace(value.Password))
        {
            throw new ValidationException("Email, username, and password are required.");
        }

        if (value.Password.Length < 8)
        {
            throw new ValidationException("Password must be at least 8 characters.");
        }
    }
}