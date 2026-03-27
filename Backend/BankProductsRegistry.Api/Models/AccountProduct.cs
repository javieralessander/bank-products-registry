using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class AccountProduct : BaseEntity
{
    public int ClientId { get; set; }
    public int FinancialProductId { get; set; }
    public int EmployeeId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset OpenDate { get; set; }
    public DateTimeOffset? MaturityDate { get; set; }
    public AccountProductStatus Status { get; set; } = AccountProductStatus.Active;

    public Client? Client { get; set; }
    public FinancialProduct? FinancialProduct { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    public ICollection<AccountProductBlock> Blocks { get; set; } = new List<AccountProductBlock>();
    public ICollection<AccountProductAuditEntry> AuditEntries { get; set; } = new List<AccountProductAuditEntry>();
    public AccountProductLimit? LimitProfile { get; set; }
    public ICollection<AccountProductLimitTemporaryAdjustment> LimitAdjustments { get; set; } = new List<AccountProductLimitTemporaryAdjustment>();
    public ICollection<AccountProductLimitHistoryEntry> LimitHistoryEntries { get; set; } = new List<AccountProductLimitHistoryEntry>();
    public ICollection<AccountProductTravelNotice> TravelNotices { get; set; } = new List<AccountProductTravelNotice>();
}
