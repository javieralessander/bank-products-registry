using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.FinancialProducts;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Route("api/financial-products")]
public sealed class FinancialProductsController(BankProductsDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FinancialProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await dbContext.FinancialProducts
            .AsNoTracking()
            .OrderBy(product => product.ProductType)
            .ThenBy(product => product.ProductName)
            .Select(product => Map(product))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FinancialProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.FinancialProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        return product is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto financiero no encontrado", $"No existe un producto financiero con el id {id}."))
            : Ok(Map(product));
    }

    [HttpPost]
    public async Task<ActionResult<FinancialProductResponse>> CreateAsync(
        [FromBody] FinancialProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        var product = new FinancialProduct
        {
            ProductName = NormalizationHelper.NormalizeName(request.ProductName),
            ProductType = request.ProductType,
            InterestRate = request.InterestRate,
            Description = NormalizationHelper.NormalizeOptionalText(request.Description),
            Status = request.Status,
            Currency = NormalizationHelper.NormalizeCode(request.Currency),
            MinimumOpeningAmount = request.MinimumOpeningAmount
        };

        dbContext.FinancialProducts.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = product.Id }, Map(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FinancialProductResponse>> UpdateAsync(
        int id,
        [FromBody] FinancialProductUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.FinancialProducts.FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto financiero no encontrado", $"No existe un producto financiero con el id {id}."));
        }

        product.ProductName = NormalizationHelper.NormalizeName(request.ProductName);
        product.ProductType = request.ProductType;
        product.InterestRate = request.InterestRate;
        product.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        product.Status = request.Status;
        product.Currency = NormalizationHelper.NormalizeCode(request.Currency);
        product.MinimumOpeningAmount = request.MinimumOpeningAmount;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(product));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<FinancialProductResponse>> PatchAsync(
        int id,
        [FromBody] FinancialProductPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProductName is null &&
            request.ProductType is null &&
            request.InterestRate is null &&
            request.Description is null &&
            request.Status is null &&
            request.Currency is null &&
            request.MinimumOpeningAmount is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Solicitud vacia",
                "Debes enviar al menos un campo para actualizar el producto financiero."));
        }

        var product = await dbContext.FinancialProducts.FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto financiero no encontrado", $"No existe un producto financiero con el id {id}."));
        }

        if (request.ProductName is not null)
        {
            product.ProductName = NormalizationHelper.NormalizeName(request.ProductName);
        }

        if (request.ProductType.HasValue)
        {
            product.ProductType = request.ProductType.Value;
        }

        if (request.InterestRate.HasValue)
        {
            product.InterestRate = request.InterestRate.Value;
        }

        if (request.Description is not null)
        {
            product.Description = NormalizationHelper.NormalizeOptionalText(request.Description);
        }

        if (request.Status.HasValue)
        {
            product.Status = request.Status.Value;
        }

        if (request.Currency is not null)
        {
            product.Currency = NormalizationHelper.NormalizeCode(request.Currency);
        }

        if (request.MinimumOpeningAmount.HasValue)
        {
            product.MinimumOpeningAmount = request.MinimumOpeningAmount.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(product));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.FinancialProducts
            .Include(currentProduct => currentProduct.AccountProducts)
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Producto financiero no encontrado", $"No existe un producto financiero con el id {id}."));
        }

        if (product.AccountProducts.Count > 0)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Producto financiero en uso",
                "No se puede eliminar el producto financiero porque esta ligado a cuentas bancarias."));
        }

        dbContext.FinancialProducts.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static FinancialProductResponse Map(FinancialProduct product) =>
        new(
            product.Id,
            product.ProductName,
            product.ProductType,
            product.InterestRate,
            product.Description,
            product.Status,
            product.Currency,
            product.MinimumOpeningAmount,
            product.CreatedAt,
            product.UpdatedAt);

    private static ProblemDetails BuildProblem(int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
}
