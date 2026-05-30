namespace VerticalSliceMinimalApi.Domain;

public sealed record Order(
    Int32 Id,
    String CustomerName,
    Decimal TotalAmount,
    DateTime CreatedAtUtc);
