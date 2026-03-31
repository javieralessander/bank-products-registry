namespace BankProductsRegistry.Frontend.Models
{
    public class FinancialProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public string ProductType { get; set; } = string.Empty;

        public decimal InterestRate { get; set; }
        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Currency { get; set; } = "DOP";
        public decimal MinimumOpeningAmount { get; set; }
    }
}