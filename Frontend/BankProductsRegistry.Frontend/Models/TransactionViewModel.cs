namespace BankProductsRegistry.Frontend.Models
{
    public class TransactionDashboardViewModel
    {
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal NetBalance { get; set; }
        public List<TransactionItemViewModel> Transactions { get; set; } = new();
    }

    public class TransactionItemViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;

        // SOLUCIÓN: El tipo viene como texto ("deposito") y el canal como número (1, 2)
        public string TransactionType { get; set; } = string.Empty;
        public int TransactionChannel { get; set; }

        public decimal Amount { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
    }
}