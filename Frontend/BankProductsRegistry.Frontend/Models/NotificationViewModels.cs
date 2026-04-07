namespace BankProductsRegistry.Frontend.Models
{
    public class NotificationDashboardViewModel
    {
        public int UnreadCount { get; set; }
        public int RiskCount { get; set; }
        public int InfoCount { get; set; }
        public int ActiveTravelsCount { get; set; }
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
    }

    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}