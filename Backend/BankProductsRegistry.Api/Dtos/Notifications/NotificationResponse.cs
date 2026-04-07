namespace BankProductsRegistry.Api.Dtos.Notifications;

public record NotificationResponse(
    int Id,
    string Title,
    string Message,
    string Type,
    DateTimeOffset CreatedAt,
    bool IsRead
);