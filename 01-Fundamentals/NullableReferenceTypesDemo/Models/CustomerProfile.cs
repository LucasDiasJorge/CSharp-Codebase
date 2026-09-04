using System.Collections.Generic;

namespace NullableReferenceTypesDemo.Models;

public sealed record CustomerProfile(
    string Id,
    string? DisplayName,
    string? Email,
    string? PhoneNumber,
    ShippingAddress? Address,
    IReadOnlyList<string>? PreferredChannels);
