namespace BankProductsRegistry.Frontend.Models
{
    public class TransactionResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int TransactionType { get; set; } // 0 = Deposito, 1 = Retiro
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; } = "Completada";
    }

    public class DashboardViewModel
    {
        public int TotalClientes { get; set; }
        public int ProductosActivos { get; set; }
        public int TotalTransacciones { get; set; }
        public decimal VolumenColocado { get; set; }
        public List<TransactionResponse> TransaccionesRecientes { get; set; } = new();
    }
}