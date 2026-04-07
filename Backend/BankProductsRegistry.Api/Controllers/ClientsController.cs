using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Clients;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/clients")]
[Authorize]
public sealed class ClientsController(
    BankProductsDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ApiControllerBase
{
    private const string GetClientByIdRoute = "GetClientById";

    [HttpGet("pending-users")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PendingClientUserResponse>>> GetPendingUsersAsync(
        CancellationToken cancellationToken)
    {
        var pendingUsers = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.ClientId == null &&
                (user.FirstName != null ||
                 user.LastName != null ||
                 user.NationalId != null ||
                 user.Phone != null ||
                 (user.UserName != null && user.Email != null && user.UserName == user.Email)) &&
                !dbContext.UserRoles.Any(userRole =>
                    userRole.UserId == user.Id &&
                    dbContext.Roles.Any(role =>
                        role.Id == userRole.RoleId &&
                        role.Name != null &&
                        role.Name != AuthRoles.ReadOnly &&
                        AuthRoles.All.Contains(role.Name))))
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var response = pendingUsers
            .Select(user =>
            {
                var (firstName, lastName) = ResolveNameParts(user);
                return new PendingClientUserResponse(
                    user.Id,
                    user.UserName ?? string.Empty,
                    user.FullName,
                    user.Email ?? string.Empty,
                    firstName,
                    lastName,
                    user.NationalId,
                    user.Phone,
                    user.IsActive);
            })
            .ToList();

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ClientResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue)
            {
                return Forbid();
            }

            var currentClient = await dbContext.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(client => client.Id == currentClientId.Value, cancellationToken);

            if (currentClient is null)
            {
                return NotFound(BuildProblem(
                    StatusCodes.Status404NotFound,
                    "Cliente no encontrado",
                    "Tu cuenta no tiene un cliente asociado valido."));
            }

            return Ok(new[] { Map(currentClient) });
        }

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
        if (IsInRole(AuthRoles.Client))
        {
            var currentClientId = GetCurrentClientId();
            if (!currentClientId.HasValue || currentClientId.Value != id)
            {
                return Forbid();
            }
        }

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

        var normalizedFirstName = NormalizationHelper.NormalizeName(request.FirstName);
        var normalizedLastName = NormalizationHelper.NormalizeName(request.LastName);
        var normalizedNationalId = NormalizationHelper.NormalizeCode(request.NationalId);
        var normalizedEmail = NormalizationHelper.NormalizeEmail(request.Email);
        var normalizedPhone = NormalizationHelper.NormalizeCode(request.Phone);

        ApplicationUser? pendingUser = null;
        if (request.RegisteredUserId.HasValue)
        {
            pendingUser = await userManager.Users
                .FirstOrDefaultAsync(user => user.Id == request.RegisteredUserId.Value, cancellationToken);

            if (pendingUser is null)
            {
                return BadRequest(BuildProblem(
                    StatusCodes.Status400BadRequest,
                    "Usuario registrado no encontrado",
                    $"No existe un usuario pendiente con el id {request.RegisteredUserId.Value}."));
            }

            if (pendingUser.ClientId.HasValue)
            {
                return Conflict(BuildProblem(
                    StatusCodes.Status409Conflict,
                    "Usuario ya vinculado",
                    "El usuario seleccionado ya tiene un cliente asociado."));
            }

            var currentRoles = await userManager.GetRolesAsync(pendingUser);
            if (currentRoles.Any(role =>
                    string.Equals(role, AuthRoles.Admin, StringComparison.Ordinal) ||
                    string.Equals(role, AuthRoles.Operator, StringComparison.Ordinal)))
            {
                return Conflict(BuildProblem(
                    StatusCodes.Status409Conflict,
                    "Usuario no elegible",
                    "No se puede vincular como cliente a un usuario administrativo u operador."));
            }

            var duplicateEmailUser = await dbContext.Users.AnyAsync(
                user => user.Id != pendingUser.Id && user.NormalizedEmail == normalizedEmail.ToUpperInvariant(),
                cancellationToken);
            if (duplicateEmailUser)
            {
                return Conflict(BuildProblem(
                    StatusCodes.Status409Conflict,
                    "Correo en uso",
                    "Ya existe otro usuario con el correo que intentas asignar al cliente."));
            }
        }

        var client = new Client
        {
            FirstName = normalizedFirstName,
            LastName = normalizedLastName,
            NationalId = normalizedNationalId,
            Email = normalizedEmail,
            Phone = normalizedPhone,
            IsActive = request.IsActive
        };

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        var linkError = default(ObjectResult);

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                dbContext.Clients.Add(client);

                dbContext.SystemNotifications.Add(new SystemNotification
                {
                    Title = "Nuevo cliente registrado",
                    Message = pendingUser is null
                        ? $"El cliente {client.FirstName} {client.LastName} fue registrado en el sistema exitosamente."
                        : $"El cliente {client.FirstName} {client.LastName} fue registrado y vinculado a un usuario digital.",
                    Type = "Sistema",
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsRead = false
                });

                await dbContext.SaveChangesAsync(cancellationToken);

                if (pendingUser is not null)
                {
                    pendingUser.FirstName = client.FirstName;
                    pendingUser.LastName = client.LastName;
                    pendingUser.NationalId = client.NationalId;
                    pendingUser.Phone = client.Phone;
                    pendingUser.FullName = $"{client.FirstName} {client.LastName}";
                    pendingUser.Email = client.Email;
                    pendingUser.UserName = client.Email;
                    pendingUser.ClientId = client.Id;
                    pendingUser.IsActive = client.IsActive;

                    var updateResult = await userManager.UpdateAsync(pendingUser);
                    if (!updateResult.Succeeded)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        linkError = BadRequest(BuildIdentityProblem(
                            StatusCodes.Status400BadRequest,
                            "No se pudo vincular el usuario registrado",
                            updateResult));
                        return;
                    }

                    var currentRoles = await userManager.GetRolesAsync(pendingUser);
                    var rolesToRemove = currentRoles
                        .Where(role => AuthRoles.All.Contains(role, StringComparer.Ordinal))
                        .Where(role => !string.Equals(role, AuthRoles.Client, StringComparison.Ordinal))
                        .ToArray();

                    if (rolesToRemove.Length > 0)
                    {
                        var removeResult = await userManager.RemoveFromRolesAsync(pendingUser, rolesToRemove);
                        if (!removeResult.Succeeded)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            linkError = BadRequest(BuildIdentityProblem(
                                StatusCodes.Status400BadRequest,
                                "No se pudo preparar el usuario para el rol Cliente",
                                removeResult));
                            return;
                        }
                    }

                    if (!currentRoles.Contains(AuthRoles.Client, StringComparer.Ordinal))
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(pendingUser, AuthRoles.Client);
                        if (!addRoleResult.Succeeded)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            linkError = BadRequest(BuildIdentityProblem(
                                StatusCodes.Status400BadRequest,
                                "No se pudo asignar el rol Cliente al usuario vinculado",
                                addRoleResult));
                            return;
                        }
                    }
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });

        if (linkError is not null)
        {
            return linkError;
        }

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
            .Include(currentClient => currentClient.User)
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

        if (client.User is not null)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente vinculado",
                "No se puede eliminar el cliente porque tiene un usuario digital asociado."));
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

    private static (string? FirstName, string? LastName) ResolveNameParts(ApplicationUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
        {
            return (user.FirstName, user.LastName);
        }

        var fullName = user.FullName?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (null, null);
        }

        var separatorIndex = fullName.IndexOf(' ');
        if (separatorIndex < 0)
        {
            return (fullName, string.Empty);
        }

        return (
            fullName[..separatorIndex].Trim(),
            fullName[(separatorIndex + 1)..].Trim());
    }
}
