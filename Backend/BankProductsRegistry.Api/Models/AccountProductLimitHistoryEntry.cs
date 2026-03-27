using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductLimitHistoryEntry : BaseEntity
{
    public int AccountProductId { get; set; }
    public int? TemporaryAdjustmentId { get; set; }
    public AccountProductLimitChangeType ChangeType { get; set; }
    public decimal? PreviousCreditLimitTotal { get; set; }
    public decimal? NewCreditLimitTotal { get; set; }
    public decimal? PreviousDailyConsumptionLimit { get; set; }
    public decimal? NewDailyConsumptionLimit { get; set; }
    public decimal? PreviousPerTransactionLimit { get; set; }
    public decimal? NewPerTransactionLimit { get; set; }
    public decimal? PreviousAtmWithdrawalLimit { get; set; }
    public decimal? NewAtmWithdrawalLimit { get; set; }
    public decimal? PreviousInternationalConsumptionLimit { get; set; }
    public decimal? NewInternationalConsumptionLimit { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? PerformedByUserId { get; set; }
    public string PerformedByUserName { get; set; } = string.Empty;

    public AccountProduct? AccountProduct { get; set; }
    public AccountProductLimitTemporaryAdjustment? TemporaryAdjustment { get; set; }
}
