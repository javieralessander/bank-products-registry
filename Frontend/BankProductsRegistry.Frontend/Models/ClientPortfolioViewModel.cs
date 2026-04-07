using System.Text.Json;

namespace BankProductsRegistry.Frontend.Models
{
    public sealed class ClientPortalPageViewModel
    {
        public int? ClientId { get; set; }
        public string? ErrorMessage { get; set; }
        public ClientPortfolioViewModel? Portfolio { get; set; }
    }

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

        /// <summary>La API puede enviar el enum como string (p. ej. activo) o número.</summary>
        public JsonElement Status { get; set; }

        public decimal Amount { get; set; }
        public DateTimeOffset OpenDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }

        public string GetStatusLabel()
        {
            if (Status.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return "—";
            }

            if (Status.ValueKind == JsonValueKind.String)
            {
                var raw = Status.GetString() ?? string.Empty;
                return raw.ToLowerInvariant() switch
                {
                    "pendiente" or "pending" => "Pendiente",
                    "activo" or "active" => "Activo",
                    "en_mora" or "delinquent" => "En mora",
                    "cerrado" or "closed" => "Cerrado",
                    "cancelado" or "cancelled" => "Cancelado",
                    _ => raw
                };
            }

            if (Status.ValueKind == JsonValueKind.Number && Status.TryGetInt32(out var n))
            {
                return n switch
                {
                    1 => "Pendiente",
                    2 => "Activo",
                    3 => "En mora",
                    4 => "Cerrado",
                    5 => "Cancelado",
                    _ => n.ToString()
                };
            }

            return "—";
        }

        public bool IsActiveStatus()
        {
            if (Status.ValueKind == JsonValueKind.String)
            {
                var s = Status.GetString()?.ToLowerInvariant();
                return s is "activo" or "active";
            }

            return Status.ValueKind == JsonValueKind.Number && Status.TryGetInt32(out var n) && n == 2;
        }
    }
}