using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Models;

public sealed class FinancialProduct : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public decimal InterestRate { get; set; }
    public string? Description { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string Currency { get; set; } = "DOP";
    public decimal MinimumOpeningAmount { get; set; }

    public ICollection<AccountProduct> AccountProducts { get; set; } = new List<AccountProduct>();
}
