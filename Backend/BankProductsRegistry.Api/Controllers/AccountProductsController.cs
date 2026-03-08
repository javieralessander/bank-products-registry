using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.AccountProducts;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Route("api/account-products")]
public sealed class AccountProductsController(BankProductsDbContext dbContext) : ControllerBase
{
    private const string GetAccountProductByIdRoute = "GetAccountProductById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var accountProducts = await dbContext.AccountProducts
            .AsNoTracking()
            .Include(accountProduct => accountProduct.Client)
            .Include(accountProduct => accountProduct.Employee)
            .Include(accountProduct => accountProduct.FinancialProduct)
            .OrderBy(accountProduct => accountProduct.AccountNumber)
            .ToListAsync(cancellationToken);

        return Ok(accountProducts.Select(Map).ToList());
    }

    [HttpGet("{id:int}", Name = GetAccountProductByIdRoute)]
    public async Task<ActionResult<AccountProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .AsNoTracking()
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        return accountProduct is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto contratado no encontrado", $"No existe un producto contratado con el id {id}."))
            : Ok(Map(accountProduct));
    }

    [HttpPost]
    public async Task<ActionResult<AccountProductResponse>> CreateAsync(
        [FromBody] AccountProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRelationsAsync(
            request.ClientId,
            request.FinancialProductId,
            request.EmployeeId,
            request.AccountNumber,
            null,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var accountProduct = new AccountProduct
        {
            ClientId = request.ClientId,
            FinancialProductId = request.FinancialProductId,
            EmployeeId = request.EmployeeId,
            AccountNumber = NormalizationHelper.NormalizeCode(request.AccountNumber),
            Amount = request.Amount,
            OpenDate = request.OpenDate,
            MaturityDate = request.MaturityDate,
            Status = request.Status
        };

        dbContext.AccountProducts.Add(accountProduct);
        await dbContext.SaveChangesAsync(cancellationToken);

        await LoadRelationsAsync(accountProduct, cancellationToken);
        return CreatedAtRoute(GetAccountProductByIdRoute, new { id = accountProduct.Id }, Map(accountProduct));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountProductResponse>> UpdateAsync(
        int id,
        [FromBody] AccountProductUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto contratado no encontrado", $"No existe un producto contratado con el id {id}."));
        }

        var validationResult = await ValidateRelationsAsync(
            request.ClientId,
            request.FinancialProductId,
            request.EmployeeId,
            request.AccountNumber,
            id,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        accountProduct.ClientId = request.ClientId;
        accountProduct.FinancialProductId = request.FinancialProductId;
        accountProduct.EmployeeId = request.EmployeeId;
        accountProduct.AccountNumber = NormalizationHelper.NormalizeCode(request.AccountNumber);
        accountProduct.Amount = request.Amount;
        accountProduct.OpenDate = request.OpenDate;
        accountProduct.MaturityDate = request.MaturityDate;
        accountProduct.Status = request.Status;

        await dbContext.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(accountProduct, cancellationToken);

        return Ok(Map(accountProduct));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<AccountProductResponse>> PatchAsync(
        int id,
        [FromBody] AccountProductPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ClientId is null &&
            request.FinancialProductId is null &&
            request.EmployeeId is null &&
            request.AccountNumber is null &&
            request.Amount is null &&
            request.OpenDate is null &&
            request.MaturityDate is null &&
            request.Status is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Solicitud vacia",
                "Debes enviar al menos un campo para actualizar el producto contratado."));
        }

        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto contratado no encontrado", $"No existe un producto contratado con el id {id}."));
        }

        var targetClientId = request.ClientId ?? accountProduct.ClientId;
        var targetProductId = request.FinancialProductId ?? accountProduct.FinancialProductId;
        var targetEmployeeId = request.EmployeeId ?? accountProduct.EmployeeId;
        var targetAccountNumber = request.AccountNumber is null
            ? accountProduct.AccountNumber
            : NormalizationHelper.NormalizeCode(request.AccountNumber);

        var validationResult = await ValidateRelationsAsync(
            targetClientId,
            targetProductId,
            targetEmployeeId,
            targetAccountNumber,
            id,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        accountProduct.ClientId = targetClientId;
        accountProduct.FinancialProductId = targetProductId;
        accountProduct.EmployeeId = targetEmployeeId;
        accountProduct.AccountNumber = targetAccountNumber;

        if (request.Amount.HasValue)
        {
            accountProduct.Amount = request.Amount.Value;
        }

        if (request.OpenDate.HasValue)
        {
            accountProduct.OpenDate = request.OpenDate.Value;
        }

        if (request.MaturityDate.HasValue)
        {
            accountProduct.MaturityDate = request.MaturityDate.Value;
        }

        if (request.Status.HasValue)
        {
            accountProduct.Status = request.Status.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(accountProduct, cancellationToken);

        return Ok(Map(accountProduct));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.Transactions)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto contratado no encontrado", $"No existe un producto contratado con el id {id}."));
        }

        if (accountProduct.Transactions.Count > 0)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Producto contratado en uso",
                "No se puede eliminar el producto contratado porque tiene transacciones asociadas."));
        }

        dbContext.AccountProducts.Remove(accountProduct);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<ActionResult?> ValidateRelationsAsync(
        int clientId,
        int financialProductId,
        int employeeId,
        string accountNumber,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Clients.AnyAsync(client => client.Id == clientId, cancellationToken))
        {
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "Cliente no valido", $"El cliente {clientId} no existe."));
        }

        if (!await dbContext.FinancialProducts.AnyAsync(product => product.Id == financialProductId, cancellationToken))
        {
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "Producto financiero no valido", $"El producto financiero {financialProductId} no existe."));
        }

        if (!await dbContext.Employees.AnyAsync(employee => employee.Id == employeeId, cancellationToken))
        {
            return BadRequest(BuildProblem(StatusCodes.Status400BadRequest, "Empleado no valido", $"El empleado {employeeId} no existe."));
        }

        var normalizedAccountNumber = NormalizationHelper.NormalizeCode(accountNumber);
        var accountAlreadyExists = await dbContext.AccountProducts.AnyAsync(
            accountProduct => accountProduct.Id != currentId && accountProduct.AccountNumber == normalizedAccountNumber,
            cancellationToken);

        if (accountAlreadyExists)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Numero de cuenta duplicado",
                "Ya existe otro producto contratado con ese numero de cuenta."));
        }

        return null;
    }

    private async Task LoadRelationsAsync(AccountProduct accountProduct, CancellationToken cancellationToken)
    {
        await dbContext.Entry(accountProduct).Reference(currentAccountProduct => currentAccountProduct.Client).LoadAsync(cancellationToken);
        await dbContext.Entry(accountProduct).Reference(currentAccountProduct => currentAccountProduct.Employee).LoadAsync(cancellationToken);
        await dbContext.Entry(accountProduct).Reference(currentAccountProduct => currentAccountProduct.FinancialProduct).LoadAsync(cancellationToken);
    }

    private static AccountProductResponse Map(AccountProduct accountProduct) =>
        new(
            accountProduct.Id,
            accountProduct.ClientId,
            accountProduct.Client is null ? string.Empty : $"{accountProduct.Client.FirstName} {accountProduct.Client.LastName}",
            accountProduct.FinancialProductId,
            accountProduct.FinancialProduct?.ProductName ?? string.Empty,
            accountProduct.EmployeeId,
            accountProduct.Employee is null ? string.Empty : $"{accountProduct.Employee.FirstName} {accountProduct.Employee.LastName}",
            accountProduct.AccountNumber,
            accountProduct.Amount,
            accountProduct.OpenDate,
            accountProduct.MaturityDate,
            accountProduct.Status,
            accountProduct.CreatedAt,
            accountProduct.UpdatedAt);

    private static ProblemDetails BuildProblem(int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
}
