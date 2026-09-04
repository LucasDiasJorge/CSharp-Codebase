using NullableReferenceTypesDemo.Models;
using NullableReferenceTypesDemo.Utilities;

namespace NullableReferenceTypesDemo.Services;

public sealed class ProfileFormatter
{
    public string CreateSummary(CustomerProfile? profile)
    {
        Guard.AgainstNull(profile, nameof(profile));
        Guard.AgainstNullOrWhiteSpace(profile.Id, nameof(profile.Id));

        string name = profile.DisplayName?.Trim() ?? "Cliente sem nome";
        string email = profile.Email?.Trim().ToLowerInvariant() ?? "email nao informado";
        string city = profile.Address?.City ?? "cidade nao informada";
        string postalCode = profile.Address?.PostalCode ?? "CEP nao informado";

        return $"{profile.Id}: {name} | {email} | {city} | {postalCode}";
    }
}
