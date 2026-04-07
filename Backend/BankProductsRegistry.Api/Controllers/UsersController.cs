using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Users;
using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/users")]
[Authorize(Policy = AuthPolicies.WriteAccess)]
public sealed class UsersController(
    BankProductsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<int>> roleManager) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserManagementResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.UserName)
            .ToListAsync(cancellationToken);

        var response = new List<UserManagementResponse>(users.Count);
        foreach (var user in users)
        {
            response.Add(await MapAsync(user));
        }

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserManagementResponse>> CreateAsync(
        [FromBody] UserCreateRequest request,
        CancellationToken cancellationToken)
    {
        var roleName = TryResolveRole(request.Role);
        if (roleName is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Rol invalido",
                $"El rol '{request.Role}' no es valido. Usa Admin, Operador, Consulta o Cliente."));
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Rol no disponible",
                $"El rol '{roleName}' no existe en la configuracion actual del sistema."));
        }

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = NormalizationHelper.NormalizeEmail(request.Email);
        int? linkedClientId = null;

        if (string.Equals(roleName, AuthRoles.Client, StringComparison.Ordinal))
        {
            if (!request.ClientId.HasValue)
            {
                return BadRequest(BuildProblem(
                    StatusCodes.Status400BadRequest,
                    "Cliente requerido",
                    "Debes indicar ClientId para crear usuarios con rol Cliente."));
            }

            var clientExists = await dbContext.Clients.AnyAsync(client => client.Id == request.ClientId.Value, cancellationToken);
            if (!clientExists)
            {
                return BadRequest(BuildProblem(
                    StatusCodes.Status400BadRequest,
                    "Cliente no encontrado",
                    $"No existe un cliente con el id {request.ClientId.Value}."));
            }

            var hasLinkedUser = await dbContext.Users.AnyAsync(user => user.ClientId == request.ClientId.Value, cancellationToken);
            if (hasLinkedUser)
            {
                return Conflict(BuildProblem(
                    StatusCodes.Status409Conflict,
                    "Cliente ya vinculado",
                    "Ese cliente ya tiene una cuenta de usuario asociada."));
            }

            linkedClientId = request.ClientId.Value;
        }

        var duplicatedUser = await dbContext.Users.AnyAsync(
            user => user.NormalizedUserName == normalizedUserName.ToUpperInvariant() ||
                    user.NormalizedEmail == normalizedEmail.ToUpperInvariant(),
            cancellationToken);

        if (duplicatedUser)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Usuario duplicado",
                "Ya existe un usuario con el mismo nombre o correo."));
        }

        var user = new ApplicationUser
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            FullName = NormalizationHelper.NormalizeName(request.FullName),
            IsActive = request.IsActive,
            EmailConfirmed = true,
            ClientId = linkedClientId
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(BuildIdentityProblem(
                StatusCodes.Status400BadRequest,
                "No se pudo crear el usuario",
                createResult));
        }

        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return BadRequest(BuildIdentityProblem(
                StatusCodes.Status400BadRequest,
                "No se pudo asignar el rol",
                roleResult));
        }

        return StatusCode(StatusCodes.Status201Created, await MapAsync(user));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserManagementResponse>> UpdateStatusAsync(
        int id,
        [FromBody] UserStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado",
                $"No existe un usuario con el id {id}."));
        }

        if (await WouldRemoveLastActiveAdminAsync(user, request.IsActive, null, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "No se puede desactivar el ultimo administrador activo del sistema."));
        }

        user.IsActive = request.IsActive;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(BuildIdentityProblem(
                StatusCodes.Status400BadRequest,
                "No se pudo actualizar el estado del usuario",
                updateResult));
        }

        await InvalidateUserSessionsAsync(user, cancellationToken);
        return Ok(await MapAsync(user));
    }

    [HttpPatch("{id:int}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserManagementResponse>> UpdateRoleAsync(
        int id,
        [FromBody] UserRoleUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var roleName = TryResolveRole(request.Role);
        if (roleName is null)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Rol invalido",
                $"El rol '{request.Role}' no es valido. Usa Admin, Operador, Consulta o Cliente."));
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Rol no disponible",
                $"El rol '{roleName}' no existe en la configuracion actual del sistema."));
        }

        var isCallerAdmin = User.IsInRole(AuthRoles.Admin);
        if (!isCallerAdmin)
        {
            if (!string.Equals(roleName, AuthRoles.Client, StringComparison.Ordinal) || !request.ClientId.HasValue)
            {
                return Forbid();
            }
        }

        var user = await userManager.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado",
                $"No existe un usuario con el id {id}."));
        }

        if (!isCallerAdmin)
        {
            var targetRoles = await userManager.GetRolesAsync(user);
            if (targetRoles.Any(currentRole =>
                    string.Equals(currentRole, AuthRoles.Admin, StringComparison.Ordinal) ||
                    string.Equals(currentRole, AuthRoles.Operator, StringComparison.Ordinal)))
            {
                return Forbid();
            }
        }

        if (string.Equals(roleName, AuthRoles.Client, StringComparison.Ordinal) &&
            !await EnsureClientLinkForClientRoleAsync(user, request.ClientId, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Cliente no vinculado",
                "Para asignar rol Cliente, primero vincula el usuario desde el mantenimiento de clientes o indica un ClientId valido."));
        }

        if (await WouldRemoveLastActiveAdminAsync(user, user.IsActive, roleName, cancellationToken))
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "No se puede cambiar el rol del ultimo administrador activo del sistema."));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles
            .Where(currentRole => AuthRoles.All.Contains(currentRole, StringComparer.Ordinal))
            .Where(currentRole => !string.Equals(currentRole, roleName, StringComparison.Ordinal))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return BadRequest(BuildIdentityProblem(
                    StatusCodes.Status400BadRequest,
                    "No se pudo actualizar el rol del usuario",
                    removeResult));
            }
        }

        if (!currentRoles.Contains(roleName, StringComparer.Ordinal))
        {
            var addResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                return BadRequest(BuildIdentityProblem(
                    StatusCodes.Status400BadRequest,
                    "No se pudo asignar el nuevo rol al usuario",
                    addResult));
            }
        }

        await InvalidateUserSessionsAsync(user, cancellationToken);
        return Ok(await MapAsync(user));
    }

    [HttpPost("{id:int}/reset-password")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordAsync(
        int id,
        [FromBody] UserResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado",
                $"No existe un usuario con el id {id}."));
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            return BadRequest(BuildIdentityProblem(
                StatusCodes.Status400BadRequest,
                "No se pudo restablecer la contrasena",
                resetResult));
        }

        await InvalidateUserSessionsAsync(user, cancellationToken);
        return NoContent();
    }

    private async Task<UserManagementResponse> MapAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserManagementResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.IsActive,
            user.EmailConfirmed,
            user.ClientId,
            roles.OrderBy(role => role).ToArray());
    }

    private async Task<bool> EnsureClientLinkForClientRoleAsync(
        ApplicationUser user,
        int? requestedClientId,
        CancellationToken cancellationToken)
    {
        if (user.ClientId.HasValue)
        {
            return true;
        }

        int? resolvedClientId = null;

        if (requestedClientId.HasValue)
        {
            var exists = await dbContext.Clients.AnyAsync(client => client.Id == requestedClientId.Value, cancellationToken);
            if (!exists)
            {
                return false;
            }

            resolvedClientId = requestedClientId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var normalizedEmail = NormalizationHelper.NormalizeEmail(user.Email);
            resolvedClientId = await dbContext.Clients
                .Where(client => client.Email == normalizedEmail)
                .Select(client => (int?)client.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!resolvedClientId.HasValue)
        {
            return false;
        }

        var alreadyLinked = await dbContext.Users.AnyAsync(
            currentUser => currentUser.Id != user.Id && currentUser.ClientId == resolvedClientId.Value,
            cancellationToken);

        if (alreadyLinked)
        {
            return false;
        }

        user.ClientId = resolvedClientId.Value;
        var updateResult = await userManager.UpdateAsync(user);
        return updateResult.Succeeded;
    }

    private async Task InvalidateUserSessionsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await userManager.UpdateSecurityStampAsync(user);

        var activeRefreshTokens = await dbContext.RefreshTokens
            .Where(token => token.ApplicationUserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        if (activeRefreshTokens.Count == 0)
        {
            return;
        }

        var revokedAt = DateTimeOffset.UtcNow;
        var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = revokedAt;
            refreshToken.RevokedByIp = NormalizationHelper.NormalizeOptionalText(remoteIpAddress);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> WouldRemoveLastActiveAdminAsync(
        ApplicationUser user,
        bool targetIsActive,
        string? targetRole,
        CancellationToken cancellationToken)
    {
        var isAdmin = await userManager.IsInRoleAsync(user, AuthRoles.Admin);
        var isCurrentActiveAdmin = user.IsActive && isAdmin;
        var willRemainAdmin = targetRole is null
            ? isAdmin
            : string.Equals(targetRole, AuthRoles.Admin, StringComparison.Ordinal);

        if (!isCurrentActiveAdmin || (targetIsActive && willRemainAdmin))
        {
            return false;
        }

        var adminRoleId = await dbContext.Roles
            .Where(role => role.Name == AuthRoles.Admin)
            .Select(role => (int?)role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminRoleId is null)
        {
            return true;
        }

        return !await dbContext.Users
            .Where(activeUser => activeUser.Id != user.Id && activeUser.IsActive)
            .Join(
                dbContext.UserRoles.Where(userRole => userRole.RoleId == adminRoleId.Value),
                activeUser => activeUser.Id,
                userRole => userRole.UserId,
                (activeUser, _) => activeUser.Id)
            .AnyAsync(cancellationToken);
    }

    private static string? TryResolveRole(string role) =>
        AuthRoles.All.FirstOrDefault(currentRole => currentRole.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase));

}
