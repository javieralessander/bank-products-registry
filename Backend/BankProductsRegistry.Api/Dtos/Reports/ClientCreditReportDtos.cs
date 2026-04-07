using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.Reports;

public sealed record ClientCreditScoreReportDto(
    int ClientId,
    string ClientName,
    int Score,
    string RiskBand,
    string Methodology,
    string Disclaimer,
    DateTimeOffset GeneratedAt,
    decimal? CreditUtilizationRatio,
    int DelinquentProducts,
    int FraudBlockEvents,
    int ActiveBlockedProducts,
    IReadOnlyCollection<ClientCreditScoreFactorDto> Factors);

public sealed record ClientCreditScoreFactorDto(
    string Factor,
    int Impact,
    string Detail);

public sealed record ClientCreditHistoryReportDto(
    int ClientId,
    string ClientName,
    string NationalId,
    string Email,
    string Phone,
    DateTimeOffset GeneratedAt,
    ClientCreditScoreReportDto Score,
    ClientCreditOverviewDto Overview,
    IReadOnlyCollection<ClientCreditAccountHistoryItemDto> Accounts,
    IReadOnlyCollection<ClientCreditHistoryEventDto> RecentEvents);

public sealed record ClientCreditOverviewDto(
    int TotalProducts,
    int ActiveProducts,
    int DelinquentProducts,
    int ActiveBlockedProducts,
    int FraudBlockEvents,
    decimal CurrentCreditExposure,
    decimal ApprovedCreditLimit,
    decimal TotalPayments,
    decimal TotalCharges,
    DateTimeOffset? OldestOpenDate);

public sealed record ClientCreditAccountHistoryItemDto(
    int AccountProductId,
    string AccountNumber,
    string ProductName,
    ProductType ProductType,
    AccountProductStatus Status,
    DateTimeOffset OpenDate,
    decimal CurrentBalance,
    decimal? ApprovedCreditLimit,
    decimal? CreditUtilizationRatio,
    int TotalTransactions,
    decimal TotalPayments,
    DateTimeOffset? LastPaymentDate,
    bool IsBlocked,
    AccountProductBlockType? ActiveBlockType);

public sealed record ClientCreditHistoryEventDto(
    DateTimeOffset OccurredAt,
    string Category,
    string Title,
    string Detail,
    string Severity);
