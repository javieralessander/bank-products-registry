using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Api.Dtos.AccountProducts;

public sealed record AccountProductTravelNoticeCreateRequest
{
    public DateTimeOffset StartsAt { get; init; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset EndsAt { get; init; }

    [Required, MaxLength(300)]
    public string Reason { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<string> Countries { get; init; } = Array.Empty<string>();
}

public sealed record AccountProductTravelNoticeCancelRequest
{
    [Required, MaxLength(300)]
    public string Reason { get; init; } = string.Empty;
}

public sealed record AccountProductTravelNoticeResponse(
    int Id,
    int AccountProductId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    IReadOnlyCollection<string> Countries,
    int? RequestedByUserId,
    string RequestedByUserName,
    DateTimeOffset? CancelledAt,
    int? CancelledByUserId,
    string? CancelledByUserName,
    string? CancellationReason,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
