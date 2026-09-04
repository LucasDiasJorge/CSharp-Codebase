using System.Collections.Generic;
using NullableReferenceTypesDemo.Models;
using NullableReferenceTypesDemo.Utilities;

namespace NullableReferenceTypesDemo.Services;

public sealed class ContactPreferenceResolver
{
    public ContactPreference Resolve(CustomerProfile profile)
    {
        Guard.AgainstNull(profile, nameof(profile));

        List<string>? channels = profile.PreferredChannels is null
            ? null
            : new List<string>(profile.PreferredChannels);

        channels ??= new List<string> { "email" };

        string channel = channels.Count == 0 ? "email" : channels[0];
        string destination = channel == "sms"
            ? profile.PhoneNumber ?? "telefone nao informado"
            : profile.Email?.Trim().ToLowerInvariant() ?? "email nao informado";

        return new ContactPreference(channel, destination);
    }
}
