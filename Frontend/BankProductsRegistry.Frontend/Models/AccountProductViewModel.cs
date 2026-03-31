namespace BankProductsRegistry.Frontend.Models
{
    public class AccountProductDashboardViewModel
    {
        public int TotalActive { get; set; }
        public decimal TotalVolume { get; set; }
        public int TotalExpiringSoon { get; set; }
        public List<AccountProductItemViewModel> Products { get; set; } = new();
    }

    public class AccountProductItemViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string FinancialProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset OpenDate { get; set; }

        // Recibimos el estado de la API como texto ("activo", "cerrado", etc.)
        public string Status { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }

        // El DTO de la API incluye detalles del bloqueo (si existe)
        public BlockSummaryViewModel? ActiveBlock { get; set; }
    }

    public class BlockSummaryViewModel
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class AccountProductCreateViewModel
    {
        public int ClientId { get; set; }
        public int FinancialProductId { get; set; }
        public int EmployeeId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OpenDate { get; set; } = DateTime.Today;
        public DateTime? MaturityDate { get; set; }
        public string Status { get; set; } = "activo";
    }
    // Modelo para Ver Detalles (Lectura)
    public class AccountProductDetailsViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int FinancialProductId { get; set; }
        public string FinancialProductName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset OpenDate { get; set; }
        public DateTimeOffset? MaturityDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsBlocked { get; set; }
        public BlockSummaryViewModel? ActiveBlock { get; set; }
    }

    // Modelo para Editar (Escritura - Hereda de Create pero añade el ID)
    public class AccountProductEditViewModel : AccountProductCreateViewModel
    {
        public int Id { get; set; }
    }


}