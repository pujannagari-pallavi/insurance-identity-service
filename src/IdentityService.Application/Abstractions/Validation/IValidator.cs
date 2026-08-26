namespace IdentityService.Application.Abstractions.Validation;

public interface IValidator<in T>
{
    void Validate(T value);
}