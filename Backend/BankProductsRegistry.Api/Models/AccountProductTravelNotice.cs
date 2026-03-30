namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductTravelNotice : BaseEntity
{
    public int AccountProductId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public DateTimeOffset? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public string? CancelledByUserName { get; set; }
    public string? CancellationReason { get; set; }

    public AccountProduct? AccountProduct { get; set; }
    public ICollection<AccountProductTravelNoticeCountry> Countries { get; set; } = new List<AccountProductTravelNoticeCountry>();
}
