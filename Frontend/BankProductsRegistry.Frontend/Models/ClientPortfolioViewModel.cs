namespace BankProductsRegistry.Frontend.Models
{
    // Esta es la bandeja combinada que le pasaremos a la vista
    public class ClientDetailsViewModel
    {
        public ClientViewModel Client { get; set; } = new();
        public ClientPortfolioViewModel Portfolio { get; set; } = new();
    }

    public class ClientPortfolioViewModel
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public List<ClientPortfolioItemViewModel> Accounts { get; set; } = new();
    }

    public class ClientPortfolioItemViewModel
    {
        public int AccountProductId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public DateTimeOffset OpenDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
    }
}