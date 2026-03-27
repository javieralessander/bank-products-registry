namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductLimitTemporaryAdjustment : BaseEntity
{
    public int AccountProductId { get; set; }
    public decimal? CreditLimitTotal { get; set; }
    public decimal? DailyConsumptionLimit { get; set; }
    public decimal? PerTransactionLimit { get; set; }
    public decimal? AtmWithdrawalLimit { get; set; }
    public decimal? InternationalConsumptionLimit { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string ApprovedByUserName { get; set; } = string.Empty;

    public AccountProduct? AccountProduct { get; set; }
    public ICollection<AccountProductLimitHistoryEntry> HistoryEntries { get; set; } = new List<AccountProductLimitHistoryEntry>();
}
