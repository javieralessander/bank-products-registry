using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Transactions;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(BankProductsDbContext dbContext) : ControllerBase
{
    private const string GetTransactionByIdRoute = "GetTransactionById";

    [HttpGet]
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
            Amount = request.Amount,
            TransactionDate = request.TransactionDate,
            Description = NormalizationHelper.NormalizeOptionalText(request.Description),
            ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber)
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(transaction).Reference(currentTransaction => currentTransaction.AccountProduct).LoadAsync(cancellationToken);
        return CreatedAtRoute(GetTransactionByIdRoute, new { id = transaction.Id }, Map(transaction));
    }

    [HttpPut("{id:int}")]
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

        ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: true);

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
        transaction.Amount = request.Amount;
        transaction.TransactionDate = request.TransactionDate;
        transaction.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        transaction.ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber);
        transaction.AccountProduct = targetAccountProduct;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<TransactionResponse>> PatchAsync(
        int id,
        [FromBody] TransactionPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AccountProductId is null &&
            request.TransactionType is null &&
            request.Amount is null &&
            request.TransactionDate is null &&
            request.Description is null &&
            request.ReferenceNumber is null)
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
        var targetAmount = request.Amount ?? transaction.Amount;

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

        ApplyTransaction(transaction.AccountProduct, transaction.TransactionType, transaction.Amount, reverse: true);

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
        transaction.Amount = targetAmount;

        if (request.TransactionDate.HasValue)
        {
            transaction.TransactionDate = request.TransactionDate.Value;
        }

        if (request.Description is not null)
        {
            transaction.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        }

        if (request.ReferenceNumber is not null)
        {
            transaction.ReferenceNumber = NormalizationHelper.NormalizeOptionalText(request.ReferenceNumber);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(transaction));
    }

    [HttpDelete("{id:int}")]
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
            transaction.Amount,
            transaction.TransactionDate,
            transaction.Description,
            transaction.ReferenceNumber,
            transaction.CreatedAt,
            transaction.UpdatedAt);

    private static ProblemDetails BuildProblem(int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
}
