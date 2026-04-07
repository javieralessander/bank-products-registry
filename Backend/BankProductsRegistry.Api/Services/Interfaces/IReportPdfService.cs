using BankProductsRegistry.Api.Dtos.Reports;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IReportPdfService
{
    byte[] BuildPortfolioPdf(ClientPortfolioReportDto dto);

    byte[] BuildCreditHistoryPdf(ClientCreditHistoryReportDto dto);

    byte[] BuildCreditScorePdf(ClientCreditScoreReportDto dto);

    byte[] BuildDashboardPdf(DashboardSummaryDto dto);
}
