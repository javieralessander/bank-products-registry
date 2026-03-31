namespace BankProductsRegistry.Frontend.Models
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalVolume { get; set; }
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
    }

    public class RecentTransactionViewModel
    {
        public int TransactionId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}