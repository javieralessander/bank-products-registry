using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.AccountProducts;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/account-products/{accountProductId:int}/blocks")]
[Authorize]
public sealed class AccountProductBlocksController(
    BankProductsDbContext dbContext,
    IAccountProductBlockService blockService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductBlockResponse>>> GetHistoryAsync(
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

        var blocks = await dbContext.AccountProductBlocks
            .AsNoTracking()
            .Where(block => block.AccountProductId == accountProductId)
            .OrderByDescending(block => block.StartsAt)
            .ThenByDescending(block => block.Id)
            .ToListAsync(cancellationToken);

        return Ok(blocks.Select(Map).ToList());
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductBlockResponse>> CreateAsync(
        int accountProductId,
        [FromBody] AccountProductBlockCreateRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts.FirstOrDefaultAsync(
            currentAccountProduct => currentAccountProduct.Id == accountProductId,
            cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        if (accountProduct.Status == AccountProductStatus.Closed || accountProduct.Status == AccountProductStatus.Cancelled)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "No se puede bloquear un producto contratado cerrado o cancelado."));
        }

        var validationProblem = ValidateCreateRequest(request);
        if (validationProblem is not null)
        {
            return BadRequest(validationProblem);
        }

        var activeBlock = await blockService.GetActiveBlockAsync(accountProductId, cancellationToken);
        if (activeBlock is not null)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Producto ya bloqueado",
                $"El producto ya tiene un bloqueo activo de tipo {ToDisplayName(activeBlock.BlockType)}."));
        }

        if (request.BlockType == AccountProductBlockType.Permanent && !User.IsInRole(AuthRoles.Admin))
        {
            return Forbid();
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        var block = new AccountProductBlock
        {
            AccountProductId = accountProductId,
            BlockType = request.BlockType,
            Reason = NormalizationHelper.NormalizeOptionalText(request.Reason) ?? string.Empty,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            AppliedByUserId = actorUserId,
            AppliedByUserName = actorUserName
        };

        if (request.BlockType == AccountProductBlockType.Permanent)
        {
            accountProduct.Status = AccountProductStatus.Cancelled;
        }

        dbContext.AccountProductBlocks.Add(block);

        // ---> NOTIFICACIùN AUTOMùTICA DE BLOQUEO <---
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = "Bloqueo de seguridad activado",
            Message = $"La cuenta/tarjeta #{accountProductId} ha sido bloqueada. Motivo: {block.Reason}.",
            Type = "Riesgo",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // --------------------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = request.BlockType switch
        {
            AccountProductBlockType.Temporary => $"Se aplico un bloqueo temporal. Motivo: {block.Reason}.",
            AccountProductBlockType.Permanent => $"Se aplico un bloqueo permanente y el producto fue marcado como cancelado. Motivo: {block.Reason}.",
            _ => $"Se aplico un bloqueo por fraude. Motivo: {block.Reason}."
        };

        await blockService.RecordAuditAsync(
            accountProductId,
            AccountProductAuditAction.BlockApplied,
            actorUserId,
            actorUserName,
            detail,
            block.Id,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, Map(block));
    }

    [HttpPost("{blockId:int}/release")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductBlockResponse>> ReleaseAsync(
        int accountProductId,
        int blockId,
        [FromBody] AccountProductBlockReleaseRequest request,
        CancellationToken cancellationToken)
    {
        var block = await dbContext.AccountProductBlocks
            .FirstOrDefaultAsync(
                currentBlock => currentBlock.Id == blockId && currentBlock.AccountProductId == accountProductId,
                cancellationToken);

        if (block is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Bloqueo no encontrado",
                $"No existe un bloqueo con el id {blockId} para el producto contratado {accountProductId}."));
        }

        if (block.BlockType == AccountProductBlockType.Permanent)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "Un bloqueo permanente no puede ser liberado."));
        }

        await blockService.GetActiveBlockAsync(accountProductId, cancellationToken);

        if (block.ReleasedAt.HasValue)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Bloqueo ya liberado",
                "El bloqueo indicado ya fue liberado o expiro automaticamente."));
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        block.ReleasedAt = DateTimeOffset.UtcNow;
        block.ReleasedByUserId = actorUserId;
        block.ReleasedByUserName = actorUserName;
        block.ReleaseReason = NormalizationHelper.NormalizeOptionalText(request.Reason);

        // ---> NOTIFICACIùN AUTOMùTICA DE DESBLOQUEO <---
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = "Producto desbloqueado",
            Message = $"El producto #{accountProductId} ha sido desbloqueado y vuelve a estar operativo.",
            Type = "Bloqueo",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // -----------------------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);

        await blockService.RecordAuditAsync(
            accountProductId,
            AccountProductAuditAction.BlockReleased,
            actorUserId,
            actorUserName,
            $"Se libero el bloqueo {ToDisplayName(block.BlockType)}. Motivo: {block.ReleaseReason}.",
            block.Id,
            cancellationToken);

        return Ok(Map(block));
    }

    private static ProblemDetails? ValidateCreateRequest(AccountProductBlockCreateRequest request)
    {
        if (request.BlockType == AccountProductBlockType.Temporary)
        {
            if (!request.EndsAt.HasValue)
            {
                return BuildProblem(
                    StatusCodes.Status400BadRequest,
                    "Datos no validos",
                    "Un bloqueo temporal requiere una fecha de fin.");
            }

            if (request.EndsAt.Value <= request.StartsAt)
            {
                return BuildProblem(
                    StatusCodes.Status400BadRequest,
                    "Datos no validos",
                    "La fecha de fin del bloqueo temporal debe ser posterior a la fecha de inicio.");
            }

            return null;
        }

        if (request.EndsAt.HasValue)
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "Solo los bloqueos temporales pueden tener fecha de fin.");
        }

        return null;
    }

    private static AccountProductBlockResponse Map(AccountProductBlock block)
    {
        var now = DateTimeOffset.UtcNow;
        var isActive = block.ReleasedAt is null &&
                       (block.BlockType != AccountProductBlockType.Temporary ||
                        !block.EndsAt.HasValue ||
                        block.EndsAt.Value > now);

        return new AccountProductBlockResponse(
            block.Id,
            block.AccountProductId,
            block.BlockType,
            block.Reason,
            block.StartsAt,
            block.EndsAt,
            block.AppliedByUserId,
            block.AppliedByUserName,
            block.ReleasedAt,
            block.ReleasedByUserId,
            block.ReleasedByUserName,
            block.ReleaseReason,
            isActive,
            block.CreatedAt,
            block.UpdatedAt);
    }

    private static string ToDisplayName(AccountProductBlockType blockType) =>
        blockType switch
        {
            AccountProductBlockType.Temporary => "temporal",
            AccountProductBlockType.Permanent => "permanente",
            _ => "fraude"
        };
}