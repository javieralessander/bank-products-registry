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

[Route("api/account-products/{accountProductId:int}/limits")]
[Authorize]
public sealed class AccountProductLimitsController(
    BankProductsDbContext dbContext,
    IAccountProductLimitService limitService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountProductLimitResponse>> GetAsync(int accountProductId, CancellationToken cancellationToken)
    {
        if (!await dbContext.AccountProducts.AnyAsync(accountProduct => accountProduct.Id == accountProductId, cancellationToken))
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        var baseLimit = await dbContext.AccountProductLimits
            .AsNoTracking()
            .FirstOrDefaultAsync(limit => limit.AccountProductId == accountProductId, cancellationToken);

        if (baseLimit is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Limites no configurados",
                "El producto contratado aun no tiene limites de consumo configurados."));
        }

        var effectiveLimits = await limitService.GetEffectiveLimitsAsync(accountProductId, DateTimeOffset.UtcNow, cancellationToken);
        return Ok(Map(baseLimit, effectiveLimits));
    }

    [HttpPut]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductLimitResponse>> UpsertAsync(
        int accountProductId,
        [FromBody] AccountProductLimitUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.LimitProfile)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == accountProductId, cancellationToken);

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
                "No se pueden configurar limites sobre un producto contratado cerrado o cancelado."));
        }

        var validationProblem = ValidateBaseLimitRequest(request);
        if (validationProblem is not null)
        {
            return BadRequest(validationProblem);
        }

        if (request.CreditLimitTotal.HasValue && accountProduct.Amount > request.CreditLimitTotal.Value)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Limite inconsistente",
                "El limite total no puede quedar por debajo del balance actual del producto contratado."));
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        var previousSnapshot = Snapshot(accountProduct.LimitProfile);
        var isNew = accountProduct.LimitProfile is null;
        var limit = accountProduct.LimitProfile ?? new AccountProductLimit
        {
            AccountProductId = accountProductId
        };

        limit.CreditLimitTotal = request.CreditLimitTotal;
        limit.DailyConsumptionLimit = request.DailyConsumptionLimit;
        limit.PerTransactionLimit = request.PerTransactionLimit;
        limit.AtmWithdrawalLimit = request.AtmWithdrawalLimit;
        limit.InternationalConsumptionLimit = request.InternationalConsumptionLimit;

        if (isNew)
        {
            dbContext.AccountProductLimits.Add(limit);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.AccountProductLimitHistoryEntries.Add(new AccountProductLimitHistoryEntry
        {
            AccountProductId = accountProductId,
            ChangeType = isNew ? AccountProductLimitChangeType.InitialConfiguration : AccountProductLimitChangeType.BaseLimitUpdated,
            PreviousCreditLimitTotal = previousSnapshot.CreditLimitTotal,
            NewCreditLimitTotal = limit.CreditLimitTotal,
            PreviousDailyConsumptionLimit = previousSnapshot.DailyConsumptionLimit,
            NewDailyConsumptionLimit = limit.DailyConsumptionLimit,
            PreviousPerTransactionLimit = previousSnapshot.PerTransactionLimit,
            NewPerTransactionLimit = limit.PerTransactionLimit,
            PreviousAtmWithdrawalLimit = previousSnapshot.AtmWithdrawalLimit,
            NewAtmWithdrawalLimit = limit.AtmWithdrawalLimit,
            PreviousInternationalConsumptionLimit = previousSnapshot.InternationalConsumptionLimit,
            NewInternationalConsumptionLimit = limit.InternationalConsumptionLimit,
            EffectiveFrom = DateTimeOffset.UtcNow,
            EffectiveTo = null,
            Reason = isNew
                ? "Configuracion inicial de limites del producto contratado."
                : "Actualizacion de limites base del producto contratado.",
            PerformedByUserId = actorUserId,
            PerformedByUserName = actorUserName
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var effectiveLimits = await limitService.GetEffectiveLimitsAsync(accountProductId, DateTimeOffset.UtcNow, cancellationToken);
        var response = Map(limit, effectiveLimits);

        return isNew
            ? StatusCode(StatusCodes.Status201Created, response)
            : Ok(response);
    }

    [HttpPost("temporary-adjustments")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductLimitTemporaryAdjustmentSummaryResponse>> CreateTemporaryAdjustmentAsync(
        int accountProductId,
        [FromBody] AccountProductLimitTemporaryAdjustmentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.LimitProfile)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == accountProductId, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        if (accountProduct.LimitProfile is null)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Limites no configurados",
                "Debes configurar primero los limites base del producto contratado."));
        }

        if (accountProduct.Status == AccountProductStatus.Closed || accountProduct.Status == AccountProductStatus.Cancelled)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "No se pueden programar ajustes temporales sobre un producto contratado cerrado o cancelado."));
        }

        var validationProblem = ValidateTemporaryAdjustmentRequest(request);
        if (validationProblem is not null)
        {
            return BadRequest(validationProblem);
        }

        if (request.CreditLimitTotal.HasValue && accountProduct.Amount > request.CreditLimitTotal.Value)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Limite inconsistente",
                "El ajuste temporal no puede dejar el limite total por debajo del balance actual del producto contratado."));
        }

        var hasOverlap = await dbContext.AccountProductLimitTemporaryAdjustments
            .AnyAsync(adjustment =>
                adjustment.AccountProductId == accountProductId &&
                adjustment.StartsAt < request.EndsAt &&
                adjustment.EndsAt > request.StartsAt,
                cancellationToken);

        if (hasOverlap)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Ajuste temporal solapado",
                "Ya existe un ajuste temporal programado que se cruza con el periodo solicitado."));
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        var baseSnapshot = Snapshot(accountProduct.LimitProfile);
        var effectiveSnapshot = ApplyAdjustment(baseSnapshot, request);
        var adjustment = new AccountProductLimitTemporaryAdjustment
        {
            AccountProductId = accountProductId,
            CreditLimitTotal = request.CreditLimitTotal,
            DailyConsumptionLimit = request.DailyConsumptionLimit,
            PerTransactionLimit = request.PerTransactionLimit,
            AtmWithdrawalLimit = request.AtmWithdrawalLimit,
            InternationalConsumptionLimit = request.InternationalConsumptionLimit,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Reason = NormalizationHelper.NormalizeOptionalText(request.Reason) ?? string.Empty,
            ApprovedByUserId = actorUserId,
            ApprovedByUserName = actorUserName
        };

        dbContext.AccountProductLimitTemporaryAdjustments.Add(adjustment);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.AccountProductLimitHistoryEntries.Add(new AccountProductLimitHistoryEntry
        {
            AccountProductId = accountProductId,
            TemporaryAdjustmentId = adjustment.Id,
            ChangeType = AccountProductLimitChangeType.TemporaryAdjustmentScheduled,
            PreviousCreditLimitTotal = baseSnapshot.CreditLimitTotal,
            NewCreditLimitTotal = effectiveSnapshot.CreditLimitTotal,
            PreviousDailyConsumptionLimit = baseSnapshot.DailyConsumptionLimit,
            NewDailyConsumptionLimit = effectiveSnapshot.DailyConsumptionLimit,
            PreviousPerTransactionLimit = baseSnapshot.PerTransactionLimit,
            NewPerTransactionLimit = effectiveSnapshot.PerTransactionLimit,
            PreviousAtmWithdrawalLimit = baseSnapshot.AtmWithdrawalLimit,
            NewAtmWithdrawalLimit = effectiveSnapshot.AtmWithdrawalLimit,
            PreviousInternationalConsumptionLimit = baseSnapshot.InternationalConsumptionLimit,
            NewInternationalConsumptionLimit = effectiveSnapshot.InternationalConsumptionLimit,
            EffectiveFrom = request.StartsAt,
            EffectiveTo = request.EndsAt,
            Reason = adjustment.Reason,
            PerformedByUserId = actorUserId,
            PerformedByUserName = actorUserName
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, Map(adjustment));
    }

    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductLimitHistoryEntryResponse>>> GetHistoryAsync(
        int accountProductId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.AccountProducts.AnyAsync(accountProduct => accountProduct.Id == accountProductId, cancellationToken))
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        var history = await dbContext.AccountProductLimitHistoryEntries
            .AsNoTracking()
            .Where(entry => entry.AccountProductId == accountProductId)
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);

        return Ok(history.Select(Map).ToList());
    }

    private static LimitSnapshot Snapshot(AccountProductLimit? limit) =>
        limit is null
            ? new LimitSnapshot(null, null, null, null, null)
            : new LimitSnapshot(
                limit.CreditLimitTotal,
                limit.DailyConsumptionLimit,
                limit.PerTransactionLimit,
                limit.AtmWithdrawalLimit,
                limit.InternationalConsumptionLimit);

    private static LimitSnapshot ApplyAdjustment(
        LimitSnapshot baseSnapshot,
        AccountProductLimitTemporaryAdjustmentCreateRequest request) =>
        new(
            request.CreditLimitTotal ?? baseSnapshot.CreditLimitTotal,
            request.DailyConsumptionLimit ?? baseSnapshot.DailyConsumptionLimit,
            request.PerTransactionLimit ?? baseSnapshot.PerTransactionLimit,
            request.AtmWithdrawalLimit ?? baseSnapshot.AtmWithdrawalLimit,
            request.InternationalConsumptionLimit ?? baseSnapshot.InternationalConsumptionLimit);

    private static ProblemDetails? ValidateBaseLimitRequest(AccountProductLimitUpsertRequest request)
    {
        if (!HasAnyLimitValue(
                request.CreditLimitTotal,
                request.DailyConsumptionLimit,
                request.PerTransactionLimit,
                request.AtmWithdrawalLimit,
                request.InternationalConsumptionLimit))
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "Debes configurar al menos un limite para el producto contratado.");
        }

        return null;
    }

    private static ProblemDetails? ValidateTemporaryAdjustmentRequest(AccountProductLimitTemporaryAdjustmentCreateRequest request)
    {
        if (!HasAnyLimitValue(
                request.CreditLimitTotal,
                request.DailyConsumptionLimit,
                request.PerTransactionLimit,
                request.AtmWithdrawalLimit,
                request.InternationalConsumptionLimit))
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "Debes indicar al menos un limite temporal a ajustar.");
        }

        if (request.EndsAt <= request.StartsAt)
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "La fecha de fin del ajuste temporal debe ser posterior a la fecha de inicio.");
        }

        return null;
    }

    private static bool HasAnyLimitValue(
        decimal? creditLimitTotal,
        decimal? dailyConsumptionLimit,
        decimal? perTransactionLimit,
        decimal? atmWithdrawalLimit,
        decimal? internationalConsumptionLimit) =>
        creditLimitTotal.HasValue ||
        dailyConsumptionLimit.HasValue ||
        perTransactionLimit.HasValue ||
        atmWithdrawalLimit.HasValue ||
        internationalConsumptionLimit.HasValue;

    private static AccountProductLimitResponse Map(AccountProductLimit baseLimit, AccountProductEffectiveLimits? effectiveLimits) =>
        new(
            baseLimit.AccountProductId,
            baseLimit.CreditLimitTotal,
            baseLimit.DailyConsumptionLimit,
            baseLimit.PerTransactionLimit,
            baseLimit.AtmWithdrawalLimit,
            baseLimit.InternationalConsumptionLimit,
            effectiveLimits?.CreditLimitTotal ?? baseLimit.CreditLimitTotal,
            effectiveLimits?.DailyConsumptionLimit ?? baseLimit.DailyConsumptionLimit,
            effectiveLimits?.PerTransactionLimit ?? baseLimit.PerTransactionLimit,
            effectiveLimits?.AtmWithdrawalLimit ?? baseLimit.AtmWithdrawalLimit,
            effectiveLimits?.InternationalConsumptionLimit ?? baseLimit.InternationalConsumptionLimit,
            effectiveLimits?.ActiveTemporaryAdjustment is null
                ? null
                : Map(effectiveLimits.ActiveTemporaryAdjustment),
            baseLimit.CreatedAt,
            baseLimit.UpdatedAt);

    private static AccountProductLimitTemporaryAdjustmentSummaryResponse Map(AccountProductLimitTemporaryAdjustment adjustment) =>
        new(
            adjustment.Id,
            adjustment.CreditLimitTotal,
            adjustment.DailyConsumptionLimit,
            adjustment.PerTransactionLimit,
            adjustment.AtmWithdrawalLimit,
            adjustment.InternationalConsumptionLimit,
            adjustment.StartsAt,
            adjustment.EndsAt,
            adjustment.Reason,
            adjustment.ApprovedByUserId,
            adjustment.ApprovedByUserName);

    private static AccountProductLimitHistoryEntryResponse Map(AccountProductLimitHistoryEntry entry) =>
        new(
            entry.Id,
            entry.AccountProductId,
            entry.TemporaryAdjustmentId,
            entry.ChangeType,
            entry.PreviousCreditLimitTotal,
            entry.NewCreditLimitTotal,
            entry.PreviousDailyConsumptionLimit,
            entry.NewDailyConsumptionLimit,
            entry.PreviousPerTransactionLimit,
            entry.NewPerTransactionLimit,
            entry.PreviousAtmWithdrawalLimit,
            entry.NewAtmWithdrawalLimit,
            entry.PreviousInternationalConsumptionLimit,
            entry.NewInternationalConsumptionLimit,
            entry.EffectiveFrom,
            entry.EffectiveTo,
            entry.Reason,
            entry.PerformedByUserId,
            entry.PerformedByUserName,
            entry.CreatedAt);

    private sealed record LimitSnapshot(
        decimal? CreditLimitTotal,
        decimal? DailyConsumptionLimit,
        decimal? PerTransactionLimit,
        decimal? AtmWithdrawalLimit,
        decimal? InternationalConsumptionLimit);
}
