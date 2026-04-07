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

[Route("api/account-products")]
[Authorize]
public sealed class AccountProductsController(
    BankProductsDbContext dbContext,
    IAccountProductBlockService blockService) : ApiControllerBase
{
    private const string GetAccountProductByIdRoute = "GetAccountProductById";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductListItemResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var accountProductsQuery = dbContext.AccountProducts
            .AsNoTracking()
            .Include(accountProduct => accountProduct.Client)
            .Include(accountProduct => accountProduct.FinancialProduct)
            .Include(accountProduct => accountProduct.Employee)
            .AsQueryable();

        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue)
            {
                return Forbid();
            }

            accountProductsQuery = accountProductsQuery.Where(accountProduct => accountProduct.ClientId == currentClientId.Value);
        }

        var accountProducts = await accountProductsQuery
            .OrderBy(accountProduct => accountProduct.AccountNumber)
            .ToListAsync(cancellationToken);

        var activeBlocks = await blockService.GetActiveBlocksAsync(
            accountProducts.Select(accountProduct => accountProduct.Id).ToArray(),
            cancellationToken);

        var response = accountProducts
            .Select(accountProduct => MapList(
                accountProduct,
                activeBlocks.GetValueOrDefault(accountProduct.Id)))
            .ToList();

        return Ok(response);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AuthRoles.InternalStaff)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductListItemResponse>>> GetPendingAsync(
        CancellationToken cancellationToken)
    {
        var accountProducts = await dbContext.AccountProducts
            .AsNoTracking()
            .Include(accountProduct => accountProduct.Client)
            .Include(accountProduct => accountProduct.FinancialProduct)
            .Include(accountProduct => accountProduct.Employee)
            .Where(accountProduct => accountProduct.Status == AccountProductStatus.Pending)
            .OrderBy(accountProduct => accountProduct.CreatedAt)
            .ToListAsync(cancellationToken);

        var activeBlocks = await blockService.GetActiveBlocksAsync(
            accountProducts.Select(accountProduct => accountProduct.Id).ToArray(),
            cancellationToken);

        var response = accountProducts
            .Select(accountProduct => MapList(
                accountProduct,
                activeBlocks.GetValueOrDefault(accountProduct.Id)))
            .ToList();

        return Ok(response);
    }

    [HttpPost("me/request")]
    [Authorize(Roles = AuthRoles.Client)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductResponse>> RequestProductFromClientAsync(
        [FromBody] AccountProductClientRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = GetCurrentClientId();
        if (!clientId.HasValue)
        {
            return Forbid();
        }

        if (!await dbContext.FinancialProducts.AnyAsync(
                product => product.Id == request.FinancialProductId,
                cancellationToken))
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Producto no valido",
                $"No existe un producto financiero con el id {request.FinancialProductId}."));
        }

        var duplicatePending = await dbContext.AccountProducts.AnyAsync(
            accountProduct =>
                accountProduct.ClientId == clientId.Value &&
                accountProduct.FinancialProductId == request.FinancialProductId &&
                accountProduct.Status == AccountProductStatus.Pending,
            cancellationToken);

        if (duplicatePending)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Solicitud duplicada",
                "Ya tienes una solicitud pendiente para este producto financiero."));
        }

        var placeholderEmployee = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.EmployeeCode == "EMP000")
            .Select(employee => (int?)employee.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await dbContext.Employees
                .AsNoTracking()
                .OrderBy(employee => employee.Id)
                .Select(employee => (int?)employee.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (!placeholderEmployee.HasValue)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Configuracion incompleta",
                "No hay empleados registrados para procesar solicitudes."));
        }

        var accountNumber = await GenerateUniqueRequestAccountNumberAsync(clientId.Value, cancellationToken);

        var accountProduct = new AccountProduct
        {
            ClientId = clientId.Value,
            FinancialProductId = request.FinancialProductId,
            EmployeeId = placeholderEmployee.Value,
            AccountNumber = accountNumber,
            Amount = request.Amount,
            OpenDate = DateTimeOffset.UtcNow,
            MaturityDate = null,
            Status = AccountProductStatus.Pending
        };

        dbContext.AccountProducts.Add(accountProduct);
        await dbContext.SaveChangesAsync(cancellationToken);

        await LoadRelationsAsync(accountProduct, cancellationToken);
        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return CreatedAtRoute(GetAccountProductByIdRoute, new { id = accountProduct.Id }, MapDetail(accountProduct, activeBlock));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductResponse>> ApprovePendingAsync(
        int id,
        [FromBody] AccountProductApproveRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {id}."));
        }

        if (accountProduct.Status != AccountProductStatus.Pending)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Estado no valido",
                "Solo se pueden aprobar solicitudes en estado pendiente."));
        }

        if (!await dbContext.Employees.AnyAsync(employee => employee.Id == request.EmployeeId, cancellationToken))
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Empleado no valido",
                $"No existe un empleado con el id {request.EmployeeId}."));
        }

        accountProduct.Status = AccountProductStatus.Active;
        accountProduct.EmployeeId = request.EmployeeId;
        accountProduct.OpenDate = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(accountProduct, cancellationToken);

        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return Ok(MapDetail(accountProduct, activeBlock));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductResponse>> RejectPendingAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {id}."));
        }

        if (accountProduct.Status != AccountProductStatus.Pending)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Estado no valido",
                "Solo se pueden rechazar solicitudes en estado pendiente."));
        }

        accountProduct.Status = AccountProductStatus.Cancelled;

        await dbContext.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(accountProduct, cancellationToken);

        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return Ok(MapDetail(accountProduct, activeBlock));
    }

    [HttpGet("{id:int}", Name = GetAccountProductByIdRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .AsNoTracking()
            .Include(currentAccountProduct => currentAccountProduct.Client)
            .Include(currentAccountProduct => currentAccountProduct.Employee)
            .Include(currentAccountProduct => currentAccountProduct.FinancialProduct)
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == id, cancellationToken);

        if (accountProduct is not null && IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue || accountProduct.ClientId != currentClientId.Value)
            {
                return Forbid();
            }
        }

        var activeBlock = accountProduct is null
            ? null
            : await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);

        return accountProduct is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto contratado no encontrado", $"No existe un producto contratado con el id {id}."))
            : Ok(MapDetail(accountProduct, activeBlock));
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductResponse>> CreateAsync(
        [FromBody] AccountProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        var resolvedAccountNumber = string.IsNullOrWhiteSpace(request.AccountNumber)
            ? await GenerateUniqueAccountNumberAsync(request.ClientId, request.FinancialProductId, cancellationToken)
            : NormalizationHelper.NormalizeCode(request.AccountNumber);

        var validationResult = await ValidateRelationsAsync(
            request.ClientId,
            request.FinancialProductId,
            request.EmployeeId,
            resolvedAccountNumber,
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
            AccountNumber = resolvedAccountNumber,
            Amount = request.Amount,
            OpenDate = request.OpenDate,
            MaturityDate = request.MaturityDate,
            Status = request.Status
        };

        dbContext.AccountProducts.Add(accountProduct);
        await dbContext.SaveChangesAsync(cancellationToken);

        await LoadRelationsAsync(accountProduct, cancellationToken);
        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return CreatedAtRoute(GetAccountProductByIdRoute, new { id = accountProduct.Id }, MapDetail(accountProduct, activeBlock));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return Ok(MapDetail(accountProduct, activeBlock));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

        var activeBlock = await blockService.GetActiveBlockAsync(accountProduct.Id, cancellationToken);
        return Ok(MapDetail(accountProduct, activeBlock));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

    private async Task<string> GenerateUniqueRequestAccountNumberAsync(int clientId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 25; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var candidate = NormalizationHelper.NormalizeCode($"REQ-{clientId}-{suffix}");
            if (candidate.Length > 30)
            {
                candidate = candidate[..30];
            }

            var exists = await dbContext.AccountProducts.AnyAsync(
                accountProduct => accountProduct.AccountNumber == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("No se pudo generar un numero de cuenta unico.");
    }

    /// <summary>
    /// Cuentas activas/creadas por personal: BR + id cliente (4) + P + id producto financiero (4) + 6 hex aleatorios.
    /// Ejemplo: BR0042P0007A3F9E2 (≤ 30 caracteres).
    /// </summary>
    private async Task<string> GenerateUniqueAccountNumberAsync(
        int clientId,
        int financialProductId,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 25; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var raw = $"BR{clientId:D4}P{financialProductId:D4}{suffix}";
            var candidate = NormalizationHelper.NormalizeCode(raw);
            if (candidate.Length > 30)
            {
                candidate = candidate[..30];
            }

            var exists = await dbContext.AccountProducts.AnyAsync(
                accountProduct => accountProduct.AccountNumber == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("No se pudo generar un numero de cuenta unico.");
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

    private static AccountProductListItemResponse MapList(AccountProduct accountProduct, AccountProductBlock? activeBlock) =>
        new(
            accountProduct.Id,
            accountProduct.ClientId,
            accountProduct.Client is null ? string.Empty : $"{accountProduct.Client.FirstName} {accountProduct.Client.LastName}",
            accountProduct.FinancialProductId,
            accountProduct.FinancialProduct?.ProductName ?? string.Empty,
            accountProduct.AccountNumber,
            accountProduct.Amount,
            accountProduct.OpenDate,
            accountProduct.Status,
            activeBlock is not null,
            MapActiveBlock(activeBlock),
            accountProduct.EmployeeId,
            accountProduct.Employee is null ? string.Empty : $"{accountProduct.Employee.FirstName} {accountProduct.Employee.LastName}");

    private static AccountProductResponse MapDetail(AccountProduct accountProduct, AccountProductBlock? activeBlock) =>
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
            accountProduct.UpdatedAt,
            activeBlock is not null,
            MapActiveBlock(activeBlock));

    private static AccountProductBlockSummaryResponse? MapActiveBlock(AccountProductBlock? activeBlock) =>
        activeBlock is null
            ? null
            : new AccountProductBlockSummaryResponse(
                activeBlock.Id,
                activeBlock.BlockType,
                activeBlock.Reason,
                activeBlock.StartsAt,
                activeBlock.EndsAt);

}
