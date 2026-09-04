using NullableReferenceTypesDemo.Models;

namespace NullableReferenceTypesDemo.Services;

public sealed class RegistrationValidator
{
    public ValidationResult Validate(CustomerProfile? profile)
    {
        if (profile is null)
        {
            return ValidationResult.Failure("Perfil nulo nao pode ser cadastrado.");
        }

        if (string.IsNullOrWhiteSpace(profile.Email))
        {
            return ValidationResult.Failure("Email e obrigatorio para cadastro.");
        }

        string normalizedEmail = profile.Email.Trim().ToLowerInvariant();

        if (!normalizedEmail.Contains('@'))
        {
            return ValidationResult.Failure("Email precisa conter @.");
        }

        if (profile.Address?.PostalCode is { Length: 8 })
        {
            return ValidationResult.Success();
        }

        return ValidationResult.Failure("CEP precisa ter 8 caracteres quando informado.");
    }
}
