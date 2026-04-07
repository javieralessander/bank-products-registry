using BankProductsRegistry.Api.Dtos.Reports;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IReportService
{
    Task<ClientPortfolioReportDto?> GetClientPortfolioAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ClientCreditHistoryReportDto?> GetClientCreditHistoryAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ClientCreditScoreReportDto?> GetClientCreditScoreAsync(int clientId, CancellationToken cancellationToken = default);

    Task<ClientTransactionStatementDto?> GetClientTransactionStatementAsync(
        int clientId,
        DateOnly fromDate,
        DateOnly toDate,
        int? accountProductId,
        CancellationToken cancellationToken = default);
}
