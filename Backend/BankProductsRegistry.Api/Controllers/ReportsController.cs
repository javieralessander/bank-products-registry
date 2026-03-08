using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("clients/{clientId:int}/portfolio")]
    public async Task<ActionResult<ClientPortfolioReportDto>> GetClientPortfolioAsync(
        int clientId,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GetClientPortfolioAsync(clientId, cancellationToken);

        return report is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Reporte no encontrado",
                Detail = $"No se pudo generar el reporte del cliente {clientId}."
            })
            : Ok(report);
    }
}
