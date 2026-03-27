using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.AccountProducts;

public sealed record AccountProductBlockCreateRequest
{
    [Required]
    public AccountProductBlockType BlockType { get; init; }

    [Required, MaxLength(300)]
    public string Reason { get; init; } = string.Empty;

    public DateTimeOffset StartsAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? EndsAt { get; init; }
}

public sealed record AccountProductBlockReleaseRequest
{
    [Required, MaxLength(300)]
    public string Reason { get; init; } = string.Empty;
}

public sealed record AccountProductBlockSummaryResponse(
    int Id,
    AccountProductBlockType BlockType,
    string Reason,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record AccountProductBlockResponse(
    int Id,
    int AccountProductId,
    AccountProductBlockType BlockType,
    string Reason,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    int? AppliedByUserId,
    string AppliedByUserName,
    DateTimeOffset? ReleasedAt,
    int? ReleasedByUserId,
    string? ReleasedByUserName,
    string? ReleaseReason,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AccountProductAuditEntryResponse(
    int Id,
    int AccountProductId,
    int? AccountProductBlockId,
    AccountProductAuditAction Action,
    int? PerformedByUserId,
    string PerformedByUserName,
    string Detail,
    DateTimeOffset CreatedAt);
