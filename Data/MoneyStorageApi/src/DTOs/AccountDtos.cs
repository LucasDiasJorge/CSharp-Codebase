using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyStorageApi.DTOs;

public class CreateAccountRequest
{
    [Required]
    [MaxLength(120)]
    public string OwnerName { get; init; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal InitialBalance { get; init; }
}

public class MoneyOperationRequest
{
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [MaxLength(200)]
    public string? Description { get; init; }
}

public record AccountResponse(
    Guid Id,
    string OwnerName,
    decimal Balance,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<MovementResponse> Movements);

public record MovementResponse(
    long Id,
    string Type,
    decimal Amount,
    string Description,
    DateTime OccurredAtUtc);
