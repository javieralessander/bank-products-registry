namespace BankProductsRegistry.Api.Dtos.Reports;

public sealed record DashboardSummaryDto(
    int TotalClients,
    int ActiveProducts,
    int TotalTransactions,
    decimal TotalVolume,
    IReadOnlyCollection<RecentTransactionDto> RecentTransactions);

public sealed record RecentTransactionDto(
    int TransactionId,
    string ClientName,
    string ProductName,
    string TransactionType,
    decimal Amount,
    DateTimeOffset Date);