using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/reports")]
[Authorize]
public sealed class ReportsController(IReportService reportService) : ApiControllerBase
{
    [HttpGet("clients/{clientId:int}/portfolio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientPortfolioReportDto>> GetClientPortfolioAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
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
        var report = await reportService.GetClientCreditScoreAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Reporte no encontrado",
                $"No se pudo generar el score interno del cliente {clientId}."))
            : Ok(report);
    }
}
