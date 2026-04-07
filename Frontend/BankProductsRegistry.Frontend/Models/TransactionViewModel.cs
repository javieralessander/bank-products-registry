using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Frontend.Models
{
    public class TransactionDashboardViewModel
    {
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal NetBalance { get; set; }
        public List<TransactionItemViewModel> Transactions { get; set; } = new();

        /// <summary>Cuentas propias del cliente para registrar movimientos (portal).</summary>
        public List<ClientAccountPickItem> ClientAccounts { get; set; } = new();
    }

    /// <summary>Cuenta contratada elegible para operaciones desde el portal cliente.</summary>
    public class ClientAccountPickItem
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string FinancialProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Formulario de transferencia / movimiento simulado (cliente).</summary>
    public class ClientTransferFormViewModel
    {
        [Required(ErrorMessage = "Selecciona un producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto.")]
        public int AccountProductId { get; set; }

        /// <summary>Valores JSON de la API: transferencia, pago, deposito, retiro.</summary>
        [Required]
        public string OperationKind { get; set; } = "transferencia";

        [Required(ErrorMessage = "Indica un monto.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(60)]
        public string? ReferenceNumber { get; set; }
    }

    public class TransactionItemViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;

        // La API serializa ambos como texto JSON (JsonStringEnumMemberName).
        public string TransactionType { get; set; } = string.Empty;
        public string TransactionChannel { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
    }
}