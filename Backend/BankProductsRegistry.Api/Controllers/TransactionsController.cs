using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Transactions;
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

[Route("api/transactions")]
[Authorize]
public sealed class TransactionsController(
    BankProductsDbContext dbContext,
    IAccountProductBlockService blockService,
    IAccountProductLimitService limitService,
    IAccountProductTravelNoticeService travelNoticeService) : ApiControllerBase
{
    private const string GetTransactionByIdRoute = "GetTransactionById";
    private const string LocalCountryCode = "DO";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TransactionResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Include(transaction => transaction.AccountProduct)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToListAsync(cancellationToken);

        return Ok(transactions.Select(Map).ToList());
    }

    [HttpGet("{id:int}", Name = GetTransactionByIdRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Transactions
            .AsNoTracking()
            .Include(currentTransaction => currentTransaction.AccountProduct)
            .FirstOrDefaultAsync(currentTransaction => currentTransaction.Id == id, cancellationToken);

        return transaction is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Transaccion no encontrada", $"No existe una transaccion con el id {id}."))
            : Ok(Map(transaction));
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionResponse>> CreateAsync(
        [FromBody] TransactionCreateRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts.FirstOrDefaultAsync(
            currentAccountProduct => currentAccountProduct.Id == request.AccountProductId,
            cancellationToken);

        if (accountProduct is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Producto contratado no valido",
                $"El producto contratado {request.AccountProductId} no existe."));
        }

        var normalizedCountryCode = NormalizeCountryCode(request.CountryCode);

        var transactionProblem = await ValidateAccountProductTransactionAsync(
            accountProduct,
            request.TransactionType,
            request.TransactionChannel,
            request.Amount,
            request.TransactionDate,
            normalizedCountryCode,
            null,
            cancellationToken);

        if (transactionProblem is not null)
        {
            return Conflict(transactionProblem);
        }

        if (!CanApplyTransaction(accountProduct.Amount, request.TransactionType, request.Amount))
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Operacion no valida",
                "La transaccion deja el balance en negativo."));
        }

        ApplyTransaction(accountProduct, request.TransactionType, request.Amount, reverse: false);

        var transaction = new BankTransaction
        {
            AccountProductId = request.AccountProductId,
            TransactionType = request.TransactionType,
            TransactionChannel = request.TransactionChannel,
            Amount = request.Amount,
            TransactionDate = request.TransactionDate,
            Description = NormalizationHelper.NormalizeOptionalText(request.Description),
            ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber),
            CountryCode = normalizedCountryCode
        };

        dbContext.Transactions.Add(transaction);

        // ---> NOTIFICACIÓN AUTOMÁTICA DE TRANSACCIÓN <---
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = "Nueva transacción registrada",
            Message = $"Se registró un movimiento de DOP {transaction.Amount:N2} en el producto #{transaction.AccountProductId}.",
            Type = "Transaccion",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // -------------------------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(transaction).Reference(currentTransaction => currentTransaction.AccountProduct).LoadAsync(cancellationToken);
        return CreatedAtRoute(GetTransactionByIdRoute, new { id = transaction.Id }, Map(transaction));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionResponse>> UpdateAsync(
        int id,
        [FromBody] TransactionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Transactions
            .Include(currentTransaction => currentTransaction.AccountProduct)
            .FirstOrDefaultAsync(currentTransaction => currentTransaction.Id == id, cancellationToken);

        if (transaction is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Transaccion no encontrada", $"No existe una transaccion con el id {id}."));
        }

        var targetAccountProduct = transaction.AccountProductId == request.AccountProductId
            ? transaction.AccountProduct
            : await dbContext.AccountProducts.FirstOrDefaultAsync(
                accountProduct => accountProduct.Id == request.AccountProductId,
                cancellationToken);

        if (targetAccountProduct is null || transaction.AccountProduct is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Producto contratado no valido",
                $"El producto contratado {request.AccountProductId} no existe."));
        }

        var normalizedCountryCode = NormalizeCountryCode(request.CountryCode);
        var sourceBlockedProblem = await ValidateAccountProductNotBlockedAsync(transaction.AccountProduct, cancellationToken);
        if (sourceBlockedProblem is not null)
        {
            return Conflict(sourceBlockedProblem);
        }

        if (targetAccountProduct.Id != transaction.AccountProduct.Id)
        {
            var targetBlockedProblem = await ValidateAccountProductNotBlockedAsync(targetAccountProduct, cancellationToken);
            if (targetBlockedProblem is not null)
            {
                return Conflict(targetBlockedProblem);
            }
        }

        ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: true);

        var limitProblem = await ValidateTransactionLimitsAsync(
            targetAccountProduct,
            request.TransactionType,
            request.TransactionChannel,
            request.Amount,
            request.TransactionDate,
            normalizedCountryCode,
            transaction.Id,
            cancellationToken);

        if (limitProblem is not null)
        {
            ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: false);
            return Conflict(limitProblem);
        }

        if (!CanApplyTransaction(targetAccountProduct.Amount, request.TransactionType, request.Amount))
        {
            ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: false);

            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Operacion no valida",
                "La transaccion actualizada deja el balance en negativo."));
        }

        ApplyTransaction(targetAccountProduct, request.TransactionType, request.Amount, reverse: false);

        transaction.AccountProductId = request.AccountProductId;
        transaction.TransactionType = request.TransactionType;
        transaction.TransactionChannel = request.TransactionChannel;
        transaction.Amount = request.Amount;
        transaction.TransactionDate = request.TransactionDate;
        transaction.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        transaction.ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber);
        transaction.CountryCode = normalizedCountryCode;
        transaction.AccountProduct = targetAccountProduct;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionResponse>> PatchAsync(
        int id,
        [FromBody] TransactionPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AccountProductId is null &&
            request.TransactionType is null &&
            request.TransactionChannel is null &&
            request.Amount is null &&
            request.TransactionDate is null &&
            request.Description is null &&
            request.ReferenceNumber is null &&
            request.CountryCode is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Solicitud vacia",
                "Debes enviar al menos un campo para actualizar la transaccion."));
        }

        var transaction = await dbContext.Transactions
            .Include(currentTransaction => currentTransaction.AccountProduct)
            .FirstOrDefaultAsync(currentTransaction => currentTransaction.Id == id, cancellationToken);

        if (transaction is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Transaccion no encontrada", $"No existe una transaccion con el id {id}."));
        }

        if (transaction.AccountProduct is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Estado no valido",
                "La transaccion no tiene un producto contratado valido."));
        }

        var targetAccountProductId = request.AccountProductId ?? transaction.AccountProductId;
        var targetTransactionType = request.TransactionType ?? transaction.TransactionType;
        var targetTransactionChannel = request.TransactionChannel ?? transaction.TransactionChannel;
        var targetAmount = request.Amount ?? transaction.Amount;
        var targetTransactionDate = request.TransactionDate ?? transaction.TransactionDate;
        var targetCountryCode = NormalizeCountryCode(request.CountryCode ?? transaction.CountryCode);

        var targetAccountProduct = targetAccountProductId == transaction.AccountProductId
            ? transaction.AccountProduct
            : await dbContext.AccountProducts.FirstOrDefaultAsync(
                accountProduct => accountProduct.Id == targetAccountProductId,
                cancellationToken);

        if (targetAccountProduct is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Producto contratado no valido",
                $"El producto contratado {targetAccountProductId} no existe."));
        }

        var currentBlockedProblem = await ValidateAccountProductNotBlockedAsync(transaction.AccountProduct, cancellationToken);
        if (currentBlockedProblem is not null)
        {
            return Conflict(currentBlockedProblem);
        }

        if (targetAccountProduct.Id != transaction.AccountProduct.Id)
        {
            var targetBlockedProblem = await ValidateAccountProductNotBlockedAsync(targetAccountProduct, cancellationToken);
            if (targetBlockedProblem is not null)
            {
                return Conflict(targetBlockedProblem);
            }
        }

        ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: true);

        var limitProblem = await ValidateTransactionLimitsAsync(
            targetAccountProduct,
            targetTransactionType,
            targetTransactionChannel,
            targetAmount,
            targetTransactionDate,
            targetCountryCode,
            transaction.Id,
            cancellationToken);

        if (limitProblem is not null)
        {
            ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: false);
            return Conflict(limitProblem);
        }

        if (!CanApplyTransaction(targetAccountProduct.Amount, targetTransactionType, targetAmount))
        {
            ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: false);

            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Operacion no valida",
                "La transaccion actualizada deja el balance en negativo."));
        }

        ApplyTransaction(targetAccountProduct, targetTransactionType, targetAmount, reverse: false);

        transaction.AccountProductId = targetAccountProductId;
        transaction.AccountProduct = targetAccountProduct;
        transaction.TransactionType = targetTransactionType;
        transaction.TransactionChannel = targetTransactionChannel;
        transaction.Amount = targetAmount;
        transaction.TransactionDate = targetTransactionDate;

        if (request.Description is not null)
        {
            transaction.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        }

        if (request.ReferenceNumber is not null)
        {
            transaction.ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber);
        }

        transaction.CountryCode = targetCountryCode;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Transactions
            .Include(currentTransaction => currentTransaction.AccountProduct)
            .FirstOrDefaultAsync(currentTransaction => currentTransaction.Id == id, cancellationToken);

        if (transaction is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Transaccion no encontrada", $"No existe una transaccion con el id {id}."));
        }

        if (transaction.AccountProduct is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Estado no valido",
                "La transaccion no tiene un producto contratado valido."));
        }

        var blockedProblem = await ValidateAccountProductNotBlockedAsync(transaction.AccountProduct, cancellationToken);
        if (blockedProblem is not null)
        {
            return Conflict(blockedProblem);
        }

        ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: true);

        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static bool CanApplyTransaction(decimal currentBalance, TransactionType transactionType, decimal amount)
    {
        var projectedBalance = transactionType == TransactionType.Deposit
            ? currentBalance + amount
            : currentBalance - amount;

        return projectedBalance >= 0;
    }

    private static void ApplyTransaction(
        AccountProduct accountProduct,
        TransactionType transactionType,
        decimal amount,
        bool reverse)
    {
        var multiplier = reverse ? -1 : 1;

        if (transactionType == TransactionType.Deposit)
        {
            accountProduct.Amount += amount * multiplier;
            return;
        }

        accountProduct.Amount -= amount * multiplier;
    }

    private static TransactionResponse Map(BankTransaction transaction) =>
        new(
            transaction.Id,
            transaction.AccountProductId,
            transaction.AccountProduct?.AccountNumber ?? string.Empty,
            transaction.TransactionType,
            transaction.TransactionChannel,
            transaction.Amount,
            transaction.TransactionDate,
            transaction.Description,
            transaction.ReferenceNumber,
            transaction.CountryCode ?? LocalCountryCode,
            !string.Equals(transaction.CountryCode ?? LocalCountryCode, LocalCountryCode, StringComparison.OrdinalIgnoreCase),
            transaction.CreatedAt,
            transaction.UpdatedAt);

    private async Task<ProblemDetails?> ValidateAccountProductNotBlockedAsync(
        AccountProduct accountProduct,
        CancellationToken cancellationToken)
    {
        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        if (activeBlock is null)
        {
            return null;
        }

        var detail = activeBlock.BlockType switch
        {
            AccountProductBlockType.Temporary =>
                $"El producto contratado esta bloqueado temporalmente hasta {activeBlock.EndsAt:O}. Motivo: {activeBlock.Reason}.",
            AccountProductBlockType.Permanent =>
                $"El producto contratado tiene un bloqueo permanente. Motivo: {activeBlock.Reason}.",
            _ =>
                $"El producto contratado esta bloqueado por fraude. Motivo: {activeBlock.Reason}."
        };

        return BuildProblem(
            StatusCodes.Status409Conflict,
            "Producto contratado bloqueado",
            detail);
    }

    private async Task<ProblemDetails?> ValidateAccountProductTransactionAsync(
        AccountProduct accountProduct,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        decimal amount,
        DateTimeOffset transactionDate,
        string countryCode,
        int? excludedTransactionId,
        CancellationToken cancellationToken)
    {
        var blockedProblem = await ValidateAccountProductNotBlockedAsync(accountProduct, cancellationToken);
        if (blockedProblem is not null)
        {
            return blockedProblem;
        }

        var travelNoticeProblem = await ValidateTravelNoticeAsync(
            accountProduct,
            transactionType,
            countryCode,
            transactionDate,
            cancellationToken);

        if (travelNoticeProblem is not null)
        {
            return travelNoticeProblem;
        }

        return await ValidateTransactionLimitsAsync(
            accountProduct,
            transactionType,
            transactionChannel,
            amount,
            transactionDate,
            countryCode,
            excludedTransactionId,
            cancellationToken);
    }

    private async Task<ProblemDetails?> ValidateTransactionLimitsAsync(
        AccountProduct accountProduct,
        TransactionType transactionType,
        TransactionChannel transactionChannel,
        decimal amount,
        DateTimeOffset transactionDate,
        string countryCode,
        int? excludedTransactionId,
        CancellationToken cancellationToken)
    {
        var limitValidation = await limitService.ValidateTransactionAsync(
            accountProduct.Id,
            accountProduct.Amount,
            transactionType,
            transactionChannel,
            amount,
            transactionDate,
            countryCode,
            excludedTransactionId,
            cancellationToken);

        return limitValidation is null
            ? null
            : BuildProblem(StatusCodes.Status409Conflict, limitValidation.Title, limitValidation.Detail);
    }

    private async Task<ProblemDetails?> ValidateTravelNoticeAsync(
        AccountProduct accountProduct,
        TransactionType transactionType,
        string countryCode,
        DateTimeOffset transactionDate,
        CancellationToken cancellationToken)
    {
        var validationResult = await travelNoticeService.ValidateInternationalTransactionAsync(
            accountProduct.Id,
            transactionType,
            countryCode,
            transactionDate,
            cancellationToken);

        return validationResult is null
            ? null
            : BuildProblem(StatusCodes.Status409Conflict, validationResult.Title, validationResult.Detail);
    }

    private static string NormalizeCountryCode(string? countryCode) =>
        string.IsNullOrWhiteSpace(countryCode)
            ? LocalCountryCode
            : NormalizationHelper.NormalizeCode(countryCode);
}