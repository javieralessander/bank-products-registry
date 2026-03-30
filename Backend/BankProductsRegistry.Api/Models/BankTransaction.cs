using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class BankTransaction : BaseEntity
{
    public int AccountProductId { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionChannel TransactionChannel { get; set; } = TransactionChannel.Branch;
    public decimal Amount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CountryCode { get; set; }

    public AccountProduct? AccountProduct { get; set; }
}
