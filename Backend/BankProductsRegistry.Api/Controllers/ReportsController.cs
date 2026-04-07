using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/reports")]
[Authorize]
public sealed class ReportsController(
    IReportService reportService,
    IReportPdfService reportPdfService,
    BankProductsDbContext dbContext) : ApiControllerBase
{
    [HttpGet("clients/{clientId:int}/portfolio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientPortfolioReportDto>> GetClientPortfolioAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientPortfolioAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el reporte del cliente {clientId}."))
            : Ok(report);
    }

    [HttpGet("clients/{clientId:int}/portfolio/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientPortfolioPdfAsync(int clientId, CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientPortfolioAsync(clientId, cancellationToken);
        if (report is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el reporte del cliente {clientId}."));
        }

        var pdf = reportPdfService.BuildPortfolioPdf(report);
        return File(pdf, "application/pdf", $"portafolio-cliente-{clientId}.pdf");
    }

    [HttpGet("clients/{clientId:int}/credit-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCreditHistoryReportDto>> GetClientCreditHistoryAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientCreditHistoryAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el historial crediticio interno del cliente {clientId}."))
            : Ok(report);
    }

    [HttpGet("clients/{clientId:int}/credit-history/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientCreditHistoryPdfAsync(int clientId, CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientCreditHistoryAsync(clientId, cancellationToken);
        if (report is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el historial crediticio interno del cliente {clientId}."));
        }

        var pdf = reportPdfService.BuildCreditHistoryPdf(report);
        return File(pdf, "application/pdf", $"historial-credito-cliente-{clientId}.pdf");
    }

    [HttpGet("clients/{clientId:int}/credit-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCreditScoreReportDto>> GetClientCreditScoreAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientCreditScoreAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el score interno del cliente {clientId}."))
            : Ok(report);
    }

    [HttpGet("clients/{clientId:int}/credit-score/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientCreditScorePdfAsync(int clientId, CancellationToken cancellationToken)
    {
        if (!EnsureClientScope(clientId))
        {
            return Forbid();
        }

        var report = await reportService.GetClientCreditScoreAsync(clientId, cancellationToken);
        if (report is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el score interno del cliente {clientId}."));
        }

        var pdf = reportPdfService.BuildCreditScorePdf(report);
        return File(pdf, "application/pdf", $"score-credito-cliente-{clientId}.pdf");
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = AuthRoles.InternalStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var summary = await BuildDashboardSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("dashboard/pdf")]
    [Authorize(Roles = AuthRoles.InternalStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardPdfAsync(CancellationToken cancellationToken)
    {
        var summary = await BuildDashboardSummaryAsync(cancellationToken);
        var pdf = reportPdfService.BuildDashboardPdf(summary);
        return File(pdf, "application/pdf", "dashboard-resumen.pdf");
    }

    private bool EnsureClientScope(int clientId)
    {
        if (!IsInRole(AuthRoles.Client))
        {
            return true;
        }

        var currentClientId = GetCurrentClientId();
        return currentClientId.HasValue && currentClientId.Value == clientId;
    }

    private async Task<DashboardSummaryDto> BuildDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        var totalClients = await dbContext.Clients.CountAsync(c => c.IsActive, cancellationToken);

        var activeProducts = await dbContext.AccountProducts
            .CountAsync(p => p.Status == AccountProductStatus.Active, cancellationToken);

        var totalTransactions = await dbContext.Transactions.CountAsync(cancellationToken);
        var totalVolume = await dbContext.Transactions.SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var recentTransactions = await dbContext.Transactions
            .Include(t => t.AccountProduct)
                .ThenInclude(ap => ap!.Client)
            .Include(t => t.AccountProduct)
                .ThenInclude(ap => ap!.FinancialProduct)
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .Select(t => new RecentTransactionDto(
                t.Id,
                t.AccountProduct != null && t.AccountProduct.Client != null
                    ? $"{t.AccountProduct.Client.FirstName} {t.AccountProduct.Client.LastName}"
                    : "Desconocido",
                t.AccountProduct != null && t.AccountProduct.FinancialProduct != null
                    ? t.AccountProduct.FinancialProduct.ProductName
                    : "Producto",
                t.TransactionType.ToString(),
                t.Amount,
                t.TransactionDate
            ))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            totalClients,
            activeProducts,
            totalTransactions,
            totalVolume,
            recentTransactions);
    }
}
