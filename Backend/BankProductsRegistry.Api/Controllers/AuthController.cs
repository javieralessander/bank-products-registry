using BankProductsRegistry.Api.Dtos.Auth;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/auth")]
[Authorize]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        if (response is null)
        {
            return Unauthorized(BuildProblem(
                StatusCodes.Status401Unauthorized,
                "Credenciales invalidas",
                "El usuario no existe, la contrasena es incorrecta o la cuenta esta inactiva."));
        }

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        // Llamada al servicio real
        var (success, errorMessage) = await authService.RegisterAsync(request, cancellationToken);

        if (!success)
        {
            return BadRequest(BuildProblem(
                StatusCodes.Status400BadRequest,
                "Error en registro",
                errorMessage));
        }

        return Ok(new { message = "Cuenta creada exitosamente" });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RefreshAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        if (response is null)
        {
            return Unauthorized(BuildProblem(
                StatusCodes.Status401Unauthorized,
                "Refresh token invalido",
                "El refresh token no existe, ya fue revocado o ha expirado."));
        }

        return Ok(response);
    }

    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(
        [FromBody] RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(BuildProblem(
                StatusCodes.Status401Unauthorized,
                "Usuario no autenticado",
                "No se pudo resolver la identidad del usuario actual."));
        }

        var revoked = await authService.RevokeRefreshTokenAsync(
            userId.Value,
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return revoked
            ? NoContent()
            : NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Refresh token no encontrado",
                "No existe un refresh token activo asociado al usuario autenticado."));
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(BuildProblem(
                StatusCodes.Status401Unauthorized,
                "Usuario no autenticado",
                "No se pudo resolver la identidad del usuario actual."));
        }

        var user = await authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        return user is null
            ? NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado",
                "La cuenta autenticada ya no existe en el sistema."))
            : Ok(user);
    }
}