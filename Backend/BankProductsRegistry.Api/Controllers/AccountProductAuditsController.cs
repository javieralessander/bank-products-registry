using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.AccountProducts;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/account-products/{accountProductId:int}/audits")]
[Authorize]
public sealed class AccountProductAuditsController(
    BankProductsDbContext dbContext,
    IAccountProductBlockService blockService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductAuditEntryResponse>>> GetAllAsync(
        int accountProductId,
        CancellationToken cancellationToken)
    {
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue)
            {
                return Forbid();
            }

            if (!await dbContext.ExistsForClientAsync(accountProductId, currentClientId.Value, cancellationToken))
            {
                return NotFound(BuildProblem(
                    StatusCodes.Status404NotFound,
                    "Producto contratado no encontrado",
                    $"No existe un producto contratado con el id {accountProductId}."));
            }
        }
        else if (!await dbContext.AccountProducts.AnyAsync(accountProduct => accountProduct.Id == accountProductId, cancellationToken))
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        await blockService.GetActiveBlockAsync(accountProductId, cancellationToken);

        var auditEntries = await dbContext.AccountProductAuditEntries
            .AsNoTracking()
            .Where(entry => entry.AccountProductId == accountProductId)
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .Select(entry => new AccountProductAuditEntryResponse(
                entry.Id,
                entry.AccountProductId,
                entry.AccountProductBlockId,
                entry.Action,
                entry.PerformedByUserId,
                entry.PerformedByUserName,
                entry.Detail,
                entry.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(auditEntries);
    }
}
