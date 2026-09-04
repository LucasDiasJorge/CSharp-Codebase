namespace NullableReferenceTypesDemo.Models;

public sealed record ShippingAddress(string City, string State, string? PostalCode);
