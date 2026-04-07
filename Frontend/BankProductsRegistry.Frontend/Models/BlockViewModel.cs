namespace BankProductsRegistry.Frontend.Models
{
    public class BlockDashboardViewModel
    {
        public int TotalActiveBlocks { get; set; }
        public int TemporaryBlocks { get; set; }
        public int PermanentBlocks { get; set; }
        public int FraudBlocks { get; set; }

        public List<BlockItemViewModel> Blocks { get; set; } = new();
    }

    public class BlockItemViewModel
    {
        public int AccountProductId { get; set; }
        public int BlockId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string FinancialProductName { get; set; } = string.Empty;

        // El tipo de bloqueo según tu API: "temporal", "permanente", "fraude"
        public string BlockType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTimeOffset StartsAt { get; set; }
    }

    public class BlockCreateViewModel
    {
        public int AccountProductId { get; set; }

        /// <summary>Cliente con una sola cuenta elegible: solo lectura + hidden.</summary>
        public bool ClientCardLocked { get; set; }

        public string? ClientCardDisplayLabel { get; set; }

        public int BlockType { get; set; } = 1; // 1: Temporal, 2: Permanente, 3: Fraude
        public string Reason { get; set; } = string.Empty;
        public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EndsAt { get; set; }
    }


}