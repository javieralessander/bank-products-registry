using BankProductsRegistry.Api.Dtos.Reports;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IReportService
{
    Task<ClientPortfolioReportDto?> GetClientPortfolioAsync(int clientId, CancellationToken cancellationToken = default);
}
