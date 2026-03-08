using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.Reports;

public sealed record ClientPortfolioReportDto(
    int ClientId,
    string ClientName,
    string Email,
    int TotalProducts,
    decimal CurrentBalance,
    decimal TotalDeposits,
    decimal TotalWithdrawals,
    IReadOnlyCollection<ClientPortfolioItemDto> Accounts);

public sealed record ClientPortfolioItemDto(
    int AccountProductId,
    string AccountNumber,
    string ProductName,
    AccountProductStatus Status,
    decimal Amount,
    DateTimeOffset OpenDate,
    int TotalTransactions,
    decimal Deposits,
    decimal Withdrawals);
