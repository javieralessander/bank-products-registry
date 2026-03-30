namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductLimit : BaseEntity
{
    public int AccountProductId { get; set; }
    public decimal? CreditLimitTotal { get; set; }
    public decimal? DailyConsumptionLimit { get; set; }
    public decimal? PerTransactionLimit { get; set; }
    public decimal? AtmWithdrawalLimit { get; set; }
    public decimal? InternationalConsumptionLimit { get; set; }

    public AccountProduct? AccountProduct { get; set; }
}
