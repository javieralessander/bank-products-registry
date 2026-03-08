using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Employees;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(BankProductsDbContext dbContext) : ControllerBase
{
    private const string GetEmployeeByIdRoute = "GetEmployeeById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var employees = await dbContext.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => Map(employee))
            .ToListAsync(cancellationToken);

        return Ok(employees);
    }

    [HttpGet("{id:int}", Name = GetEmployeeByIdRoute)]
    public async Task<ActionResult<EmployeeResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == id, cancellationToken);

        return employee is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Empleado no encontrado", $"No existe un empleado con el id {id}."))
            : Ok(Map(employee));
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> CreateAsync(
        [FromBody] EmployeeCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (await ExistsDuplicateAsync(request.EmployeeCode, request.Email, null, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Empleado duplicado",
                "Ya existe un empleado con el mismo codigo o correo."));
        }

        var employee = new Employee
        {
            FirstName = NormalizationHelper.NormalizeName(request.FirstName),
            LastName = NormalizationHelper.NormalizeName(request.LastName),
            EmployeeCode = NormalizationHelper.NormalizeCode(request.EmployeeCode),
            Email = NormalizationHelper.NormalizeEmail(request.Email),
            Department = NormalizationHelper.NormalizeName(request.Department),
            IsActive = request.IsActive
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtRoute(GetEmployeeByIdRoute, new { id = employee.Id }, Map(employee));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> UpdateAsync(
        int id,
        [FromBody] EmployeeUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == id, cancellationToken);
        if (employee is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Empleado no encontrado", $"No existe un empleado con el id {id}."));
        }

        if (await ExistsDuplicateAsync(request.EmployeeCode, request.Email, id, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Empleado duplicado",
                "Ya existe un empleado con el mismo codigo o correo."));
        }

        employee.FirstName = NormalizationHelper.NormalizeName(request.FirstName);
        employee.LastName = NormalizationHelper.NormalizeName(request.LastName);
        employee.EmployeeCode = NormalizationHelper.NormalizeCode(request.EmployeeCode);
        employee.Email = NormalizationHelper.NormalizeEmail(request.Email);
        employee.Department = NormalizationHelper.NormalizeName(request.Department);
        employee.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(employee));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> PatchAsync(
        int id,
        [FromBody] EmployeePatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.FirstName is null &&
            request.LastName is null &&
            request.EmployeeCode is null &&
            request.Email is null &&
            request.Department is null &&
            request.IsActive is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Solicitud vacia",
                "Debes enviar al menos un campo para actualizar el empleado."));
        }

        var employee = await dbContext.Employees.FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == id, cancellationToken);
        if (employee is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Empleado no encontrado", $"No existe un empleado con el id {id}."));
        }

        var targetCode = request.EmployeeCode is null ? employee.EmployeeCode : NormalizationHelper.NormalizeCode(request.EmployeeCode);
        var targetEmail = request.Email is null ? employee.Email : NormalizationHelper.NormalizeEmail(request.Email);

        if (await ExistsDuplicateAsync(targetCode, targetEmail, id, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Empleado duplicado",
                "Ya existe un empleado con el mismo codigo o correo."));
        }

        if (request.FirstName is not null)
        {
            employee.FirstName = NormalizationHelper.NormalizeName(request.FirstName);
        }

        if (request.LastName is not null)
        {
            employee.LastName = NormalizationHelper.NormalizeName(request.LastName);
        }

        if (request.EmployeeCode is not null)
        {
            employee.EmployeeCode = targetCode;
        }

        if (request.Email is not null)
        {
            employee.Email = targetEmail;
        }

        if (request.Department is not null)
        {
            employee.Department = NormalizationHelper.NormalizeName(request.Department);
        }

        if (request.IsActive.HasValue)
        {
            employee.IsActive = request.IsActive.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(employee));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .Include(currentEmployee => currentEmployee.ManagedProducts)
            .FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == id, cancellationToken);

        if (employee is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Empleado no encontrado", $"No existe un empleado con el id {id}."));
        }

        if (employee.ManagedProducts.Count > 0)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Empleado en uso",
                "No se puede eliminar el empleado porque tiene productos bancarios asignados."));
        }

        dbContext.Employees.Remove(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<bool> ExistsDuplicateAsync(
        string employeeCode,
        string email,
        int? currentId,
        CancellationToken cancellationToken) =>
        dbContext.Employees.AnyAsync(
            employee =>
                employee.Id != currentId &&
                (employee.EmployeeCode == NormalizationHelper.NormalizeCode(employeeCode) ||
                 employee.Email == NormalizationHelper.NormalizeEmail(email)),
            cancellationToken);

    private static EmployeeResponse Map(Employee employee) =>
        new(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.EmployeeCode,
            employee.Email,
            employee.Department,
            employee.IsActive,
            employee.CreatedAt,
            employee.UpdatedAt);

    private static ProblemDetails BuildProblem(int statusCode, string title, string detail) =>
        new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
}
