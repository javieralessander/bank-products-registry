using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.Transactions;

public record TransactionCreateRequest
{
    [Required]
    public int AccountProductId { get; init; }

    [Required]
    public TransactionType TransactionType { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    public DateTimeOffset TransactionDate { get; init; } = DateTimeOffset.UtcNow;

    [MaxLength(300)]
    public string? Description { get; init; }

    [MaxLength(60)]
    public string? ReferenceNumber { get; init; }
}

public sealed record TransactionUpdateRequest : TransactionCreateRequest;

public sealed record TransactionPatchRequest
{
    public int? AccountProductId { get; init; }
    public TransactionType? TransactionType { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; init; }

    public DateTimeOffset? TransactionDate { get; init; }

    [MaxLength(300)]
    public string? Description { get; init; }

    [MaxLength(60)]
    public string? ReferenceNumber { get; init; }
}

public sealed record TransactionResponse(
    int Id,
    int AccountProductId,
    string AccountNumber,
    TransactionType TransactionType,
    decimal Amount,
    DateTimeOffset TransactionDate,
    string? Description,
    string? ReferenceNumber,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
