using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Clients;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/clients")]
[Authorize]
public sealed class ClientsController(BankProductsDbContext dbContext) : ApiControllerBase
{
    private const string GetClientByIdRoute = "GetClientById";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ClientResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var clients = await dbContext.Clients
            .AsNoTracking()
            .OrderBy(client => client.LastName)
            .ThenBy(client => client.FirstName)
            .Select(client => Map(client))
            .ToListAsync(cancellationToken);

        return Ok(clients);
    }

    [HttpGet("{id:int}", Name = GetClientByIdRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(currentClient => currentClient.Id == id, cancellationToken);

        return client is null
            ? NotFound(BuildProblem(StatusCodes.Status404NotFound, "Cliente no encontrado", $"No existe un cliente con el id {id}."))
            : Ok(Map(client));
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> CreateAsync(
        [FromBody] ClientCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (await ExistsDuplicateAsync(request.NationalId, request.Email, null, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente duplicado",
                "Ya existe un cliente con la misma cedula o correo."));
        }

        var client = new Client
        {
            FirstName = NormalizationHelper.NormalizeName(request.FirstName),
            LastName = NormalizationHelper.NormalizeName(request.LastName),
            NationalId = NormalizationHelper.NormalizeCode(request.NationalId),
            Email = NormalizationHelper.NormalizeEmail(request.Email),
            Phone = NormalizationHelper.NormalizeCode(request.Phone),
            IsActive = request.IsActive
        };

        dbContext.Clients.Add(client);

        // ---> NOTIFICACIÓN AUTOMÁTICA <---
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = "Nuevo cliente registrado",
            Message = $"El cliente {client.FirstName} {client.LastName} fue registrado en el sistema exitosamente.",
            Type = "Sistema",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // ---------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtRoute(GetClientByIdRoute, new { id = client.Id }, Map(client));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> UpdateAsync(
        int id,
        [FromBody] ClientUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients.FirstOrDefaultAsync(currentClient => currentClient.Id == id, cancellationToken);
        if (client is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Cliente no encontrado", $"No existe un cliente con el id {id}."));
        }

        if (await ExistsDuplicateAsync(request.NationalId, request.Email, id, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente duplicado",
                "Ya existe un cliente con la misma cedula o correo."));
        }

        client.FirstName = NormalizationHelper.NormalizeName(request.FirstName);
        client.LastName = NormalizationHelper.NormalizeName(request.LastName);
        client.NationalId = NormalizationHelper.NormalizeCode(request.NationalId);
        client.Email = NormalizationHelper.NormalizeEmail(request.Email);
        client.Phone = NormalizationHelper.NormalizeCode(request.Phone);
        client.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(client));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> PatchAsync(
        int id,
        [FromBody] ClientPatchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.FirstName is null &&
            request.LastName is null &&
            request.NationalId is null &&
            request.Email is null &&
            request.Phone is null &&
            request.IsActive is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Solicitud vacia",
                "Debes enviar al menos un campo para actualizar el cliente."));
        }

        var client = await dbContext.Clients.FirstOrDefaultAsync(currentClient => currentClient.Id == id, cancellationToken);
        if (client is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Cliente no encontrado", $"No existe un cliente con el id {id}."));
        }

        var targetNationalId = request.NationalId is null ? client.NationalId : NormalizationHelper.NormalizeCode(request.NationalId);
        var targetEmail = request.Email is null ? client.Email : NormalizationHelper.NormalizeEmail(request.Email);

        if (await ExistsDuplicateAsync(targetNationalId, targetEmail, id, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente duplicado",
                "Ya existe un cliente con la misma cedula o correo."));
        }

        if (request.FirstName is not null)
        {
            client.FirstName = NormalizationHelper.NormalizeName(request.FirstName);
        }

        if (request.LastName is not null)
        {
            client.LastName = NormalizationHelper.NormalizeName(request.LastName);
        }

        if (request.NationalId is not null)
        {
            client.NationalId = targetNationalId;
        }

        if (request.Email is not null)
        {
            client.Email = targetEmail;
        }

        if (request.Phone is not null)
        {
            client.Phone = NormalizationHelper.NormalizeCode(request.Phone);
        }

        if (request.IsActive.HasValue)
        {
            client.IsActive = request.IsActive.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(client));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients
            .Include(currentClient => currentClient.AccountProducts)
            .FirstOrDefaultAsync(currentClient => currentClient.Id == id, cancellationToken);

        if (client is null)
        {
            return NotFound(BuildProblem(StatusCodes.Status404NotFound, "Cliente no encontrado", $"No existe un cliente con el id {id}."));
        }

        if (client.AccountProducts.Count > 0)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente en uso",
                "No se puede eliminar el cliente porque tiene productos bancarios registrados."));
        }

        dbContext.Clients.Remove(client);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<bool> ExistsDuplicateAsync(
        string nationalId,
        string email,
        int? currentId,
        CancellationToken cancellationToken) =>
        dbContext.Clients.AnyAsync(
            client =>
                client.Id != currentId &&
                (client.NationalId == NormalizationHelper.NormalizeCode(nationalId) ||
                 client.Email == NormalizationHelper.NormalizeEmail(email)),
            cancellationToken);

    private static ClientResponse Map(Client client) =>
        new(
            client.Id,
            client.FirstName,
            client.LastName,
            client.NationalId,
            client.Email,
            client.Phone,
            client.IsActive,
            client.CreatedAt,
            client.UpdatedAt);
}