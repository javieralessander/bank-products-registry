using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductBlock : BaseEntity
{
    public int AccountProductId { get; set; }
    public AccountProductBlockType BlockType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public int? AppliedByUserId { get; set; }
    public string AppliedByUserName { get; set; } = string.Empty;
    public DateTimeOffset? ReleasedAt { get; set; }
    public int? ReleasedByUserId { get; set; }
    public string? ReleasedByUserName { get; set; }
    public string? ReleaseReason { get; set; }

    public AccountProduct? AccountProduct { get; set; }
}
