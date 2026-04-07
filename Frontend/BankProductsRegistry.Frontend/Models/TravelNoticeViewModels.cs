namespace BankProductsRegistry.Frontend.Models
{
    public class TravelNoticeDashboardViewModel
    {
        public int ActiveTravels { get; set; }
        public int UpcomingTravels { get; set; }
        public int FinishedTravels { get; set; }
        public List<TravelNoticeCardViewModel> Notices { get; set; } = new();
    }

    public class TravelNoticeCardViewModel
    {
        public int AccountProductId { get; set; }
        public int NoticeId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string[] Countries { get; set; } = Array.Empty<string>();
        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
    }

    public class TravelNoticeCreateViewModel
    {
        public int AccountProductId { get; set; }

        /// <summary>Si es true (cliente con una sola cuenta activa), la vista muestra solo lectura + hidden.</summary>
        public bool ClientCardLocked { get; set; }

        /// <summary>Texto mostrado cuando <see cref="ClientCardLocked"/> es true.</summary>
        public string? ClientCardDisplayLabel { get; set; }

        public string CountryCode { get; set; } = string.Empty;
        public string CitiesOrReason { get; set; } = string.Empty;
        public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset EndsAt { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    }


}