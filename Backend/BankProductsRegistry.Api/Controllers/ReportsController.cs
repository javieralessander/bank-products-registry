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
    BankProductsDbContext dbContext) : ApiControllerBase
{
    [HttpGet("clients/{clientId:int}/portfolio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientPortfolioReportDto>> GetClientPortfolioAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue || currentClientId.Value != clientId)
            {
                return Forbid();
            }
        }

        var report = await reportService.GetClientPortfolioAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el reporte del cliente {clientId}."))
            : Ok(report);
    }

    [HttpGet("clients/{clientId:int}/credit-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCreditHistoryReportDto>> GetClientCreditHistoryAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue || currentClientId.Value != clientId)
            {
                return Forbid();
            }
        }

        var report = await reportService.GetClientCreditHistoryAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el historial crediticio interno del cliente {clientId}."))
            : Ok(report);
    }

    [HttpGet("clients/{clientId:int}/credit-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientCreditScoreReportDto>> GetClientCreditScoreAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue || currentClientId.Value != clientId)
            {
                return Forbid();
            }
        }

        var report = await reportService.GetClientCreditScoreAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el score interno del cliente {clientId}."))
            : Ok(report);
    }

    // --- NUEVO ENDPOINT PARA EL DASHBOARD ---
    [HttpGet("dashboard")]
    [Authorize(Roles = AuthRoles.InternalStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken cancellationToken)
    {
        // 1. Contar Clientes Activos
        var totalClients = await dbContext.Clients.CountAsync(c => c.IsActive, cancellationToken);

        // 2. Contar Productos Activos
        var activeProducts = await dbContext.AccountProducts
            .CountAsync(p => p.Status == AccountProductStatus.Active, cancellationToken);

        // 3. Contar y Sumar Transacciones (Usamos decimal? por si la tabla est vaca)
        var totalTransactions = await dbContext.Transactions.CountAsync(cancellationToken);
        var totalVolume = await dbContext.Transactions.SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        // 4. Obtener las 5 transacciones ms recientes con los datos de Cliente y Producto
        var recentTransactions = await dbContext.Transactions
            .Include(t => t.AccountProduct)
                .ThenInclude(ap => ap.Client)
            .Include(t => t.AccountProduct)
                .ThenInclude(ap => ap.FinancialProduct)
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

        var summary = new DashboardSummaryDto(
            totalClients,
            activeProducts,
            totalTransactions,
            totalVolume,
            recentTransactions);

        return Ok(summary);
    }
}