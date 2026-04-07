using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Frontend.Models
{
    public class EmployeeOptionViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class AccountProductsPendingPageViewModel
    {
        public List<AccountProductItemViewModel> Pending { get; set; } = new();
        public List<EmployeeOptionViewModel> Employees { get; set; } = new();
    }

    public class FinancialProductPickItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ProductRequestFormViewModel
    {
        [Required(ErrorMessage = "Selecciona un producto.")]
        [Display(Name = "Producto financiero")]
        public int FinancialProductId { get; set; }

        [Range(0, 9999999999999999.99, ErrorMessage = "El monto debe ser mayor o igual a cero.")]
        [Display(Name = "Monto solicitado (DOP)")]
        public decimal Amount { get; set; }
    }

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
        public int ClientId { get; set; }
        public int FinancialProductId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string FinancialProductName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
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

        /// <summary>Dejar vacío: la API asigna número automático (nomenclatura BR…).</summary>
        public string? AccountNumber { get; set; }
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

    public class AccountProductLimitsPageViewModel
    {
        public AccountProductDetailsViewModel Product { get; set; } = new();
        public AccountProductLimitSummaryViewModel? CurrentLimits { get; set; }
        public List<AccountProductLimitHistoryEntryViewModel> History { get; set; } = new();
        public AccountProductLimitEditViewModel BaseForm { get; set; } = new();
        public AccountProductLimitTemporaryAdjustmentViewModel TemporaryAdjustmentForm { get; set; } = new();
        public bool CanEdit { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AccountProductLimitSummaryViewModel
    {
        public int AccountProductId { get; set; }
        public decimal? BaseCreditLimitTotal { get; set; }
        public decimal? BaseDailyConsumptionLimit { get; set; }
        public decimal? BasePerTransactionLimit { get; set; }
        public decimal? BaseAtmWithdrawalLimit { get; set; }
        public decimal? BaseInternationalConsumptionLimit { get; set; }
        public decimal? EffectiveCreditLimitTotal { get; set; }
        public decimal? EffectiveDailyConsumptionLimit { get; set; }
        public decimal? EffectivePerTransactionLimit { get; set; }
        public decimal? EffectiveAtmWithdrawalLimit { get; set; }
        public decimal? EffectiveInternationalConsumptionLimit { get; set; }
        public AccountProductLimitTemporaryAdjustmentSummaryViewModel? ActiveTemporaryAdjustment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class AccountProductLimitTemporaryAdjustmentSummaryViewModel
    {
        public int Id { get; set; }
        public decimal? CreditLimitTotal { get; set; }
        public decimal? DailyConsumptionLimit { get; set; }
        public decimal? PerTransactionLimit { get; set; }
        public decimal? AtmWithdrawalLimit { get; set; }
        public decimal? InternationalConsumptionLimit { get; set; }
        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int? ApprovedByUserId { get; set; }
        public string ApprovedByUserName { get; set; } = string.Empty;
    }

    public class AccountProductLimitHistoryEntryViewModel
    {
        public int Id { get; set; }
        public int AccountProductId { get; set; }
        public int? TemporaryAdjustmentId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public decimal? PreviousCreditLimitTotal { get; set; }
        public decimal? NewCreditLimitTotal { get; set; }
        public decimal? PreviousDailyConsumptionLimit { get; set; }
        public decimal? NewDailyConsumptionLimit { get; set; }
        public decimal? PreviousPerTransactionLimit { get; set; }
        public decimal? NewPerTransactionLimit { get; set; }
        public decimal? PreviousAtmWithdrawalLimit { get; set; }
        public decimal? NewAtmWithdrawalLimit { get; set; }
        public decimal? PreviousInternationalConsumptionLimit { get; set; }
        public decimal? NewInternationalConsumptionLimit { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int? PerformedByUserId { get; set; }
        public string PerformedByUserName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }

        public string GetChangeTypeLabel()
        {
            var normalized = ChangeType?.Trim().ToLowerInvariant();
            return normalized switch
            {
                "initialconfiguration" or "configuracion_inicial" => "Configuración inicial",
                "baselimitupdated" or "actualizacion_base" => "Actualización límites base",
                "temporaryadjustmentscheduled" or "ajuste_temporal" => "Ajuste temporal",
                _ => string.IsNullOrWhiteSpace(ChangeType) ? "Cambio registrado" : ChangeType
            };
        }
    }

    public class AccountProductLimitEditViewModel
    {
        [Display(Name = "Limite total de credito")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "Ingresa un monto valido mayor a cero.")]
        public decimal? CreditLimitTotal { get; set; }

        [Display(Name = "Consumo diario")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "Ingresa un monto valido mayor a cero.")]
        public decimal? DailyConsumptionLimit { get; set; }

        [Display(Name = "Por transaccion")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "Ingresa un monto valido mayor a cero.")]
        public decimal? PerTransactionLimit { get; set; }

        [Display(Name = "Retiro por cajero")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "Ingresa un monto valido mayor a cero.")]
        public decimal? AtmWithdrawalLimit { get; set; }

        [Display(Name = "Consumo internacional")]
        [Range(0.01, 9999999999999999.99, ErrorMessage = "Ingresa un monto valido mayor a cero.")]
        public decimal? InternationalConsumptionLimit { get; set; }
    }

    public class AccountProductLimitTemporaryAdjustmentViewModel : AccountProductLimitEditViewModel
    {
        [Display(Name = "Inicio del ajuste")]
        public DateTime StartsAtLocal { get; set; } = DateTime.Now;

        [Display(Name = "Fin del ajuste")]
        public DateTime EndsAtLocal { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Indica el motivo del ajuste.")]
        [StringLength(300, ErrorMessage = "El motivo no puede exceder 300 caracteres.")]
        [Display(Name = "Motivo")]
        public string Reason { get; set; } = string.Empty;
    }

    // Modelo para Editar (Escritura - Hereda de Create pero añade el ID)
    public class AccountProductEditViewModel : AccountProductCreateViewModel
    {
        public int Id { get; set; }
    }


}
