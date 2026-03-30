using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductAuditEntry : BaseEntity
{
    public int AccountProductId { get; set; }
    public int? AccountProductBlockId { get; set; }
    public AccountProductAuditAction Action { get; set; }
    public int? PerformedByUserId { get; set; }
    public string PerformedByUserName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;

    public AccountProduct? AccountProduct { get; set; }
    public AccountProductBlock? AccountProductBlock { get; set; }
}
