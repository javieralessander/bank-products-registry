using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.Transactions;

public record TransactionCreateRequest
{
    [Required]
    public int AccountProductId { get; init; }

    [Required]
    public TransactionType TransactionType { get; init; }

    public TransactionChannel TransactionChannel { get; init; } = TransactionChannel.Branch;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    public DateTimeOffset TransactionDate { get; init; } = DateTimeOffset.UtcNow;

    [MaxLength(300)]
    public string? Description { get; init; }

    [MaxLength(60)]
    public string? ReferenceNumber { get; init; }

    [MaxLength(2)]
    public string? CountryCode { get; init; }
}

public sealed record TransactionUpdateRequest : TransactionCreateRequest;

public sealed record TransactionPatchRequest
{
    public int? AccountProductId { get; init; }
    public TransactionType? TransactionType { get; init; }
    public TransactionChannel? TransactionChannel { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; init; }

    public DateTimeOffset? TransactionDate { get; init; }

    [MaxLength(300)]
    public string? Description { get; init; }

    [MaxLength(60)]
    public string? ReferenceNumber { get; init; }

    [MaxLength(2)]
    public string? CountryCode { get; init; }
}

public sealed record TransactionResponse(
    int Id,
    int AccountProductId,
    string AccountNumber,
    TransactionType TransactionType,
    TransactionChannel TransactionChannel,
    decimal Amount,
    DateTimeOffset TransactionDate,
    string? Description,
    string? ReferenceNumber,
    string CountryCode,
    bool IsInternational,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
