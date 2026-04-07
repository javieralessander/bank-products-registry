using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Auth;
using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services.Auth;

public sealed class AuthService(
    BankProductsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        string? remoteIpAddress,
        CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(request.UserNameOrEmail);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsValid)
        {
            return null;
        }

        return await IssueTokensAsync(user, remoteIpAddress, cancellationToken);
    }

    // ====================================================================
    // MÉTODO DE REGISTRO DE CLIENTES
    // ====================================================================
    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizationHelper.NormalizeEmail(request.Email);

        // 1. Verificar si ya existe un usuario con ese correo o username
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return (false, "El correo electrónico proporcionado ya está en uso.");
        }

        // 2. Crear la entidad del nuevo usuario
        var newUser = new ApplicationUser
        {
            UserName = normalizedEmail, // Usamos el correo como username
            Email = normalizedEmail,
            FullName = $"{request.Nombre.Trim()} {request.Apellido.Trim()}",
            IsActive = true
        };

        // 3. Guardar en Base de Datos con contraseña hasheada
        var result = await userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
        {
            // Devuelve los errores de Identity (ej. Password requiere mayúsculas, etc.)
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        // 4. Asignar rol de "Client" por defecto
        await userManager.AddToRoleAsync(newUser, "consulta");

        return (true, string.Empty);
    }
    // ====================================================================

    public async Task<AuthResponse?> RefreshAsync(
        RefreshTokenRequest request,
        string? remoteIpAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
            .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.User is null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return null;
        }

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = NormalizationHelper.NormalizeOptionalText(remoteIpAddress);

        var response = await IssueTokensAsync(storedToken.User, remoteIpAddress, cancellationToken);
        if (response is null)
        {
            return null;
        }

        storedToken.ReplacedByTokenHash = tokenService.HashRefreshToken(response.RefreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<bool> RevokeRefreshTokenAsync(
        int userId,
        RevokeRefreshTokenRequest request,
        string? remoteIpAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.ApplicationUserId == userId && refreshToken.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return false;
        }

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = NormalizationHelper.NormalizeOptionalText(remoteIpAddress);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AuthenticatedUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return MapUser(user, roles);
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            return null;
        }

        var normalizedValue = userNameOrEmail.Trim();

        if (normalizedValue.Contains('@'))
        {
            var byEmail = await userManager.FindByEmailAsync(NormalizationHelper.NormalizeEmail(normalizedValue));
            if (byEmail is not null)
            {
                return byEmail;
            }
        }

        return await userManager.FindByNameAsync(normalizedValue);
    }

    private async Task<AuthResponse?> IssueTokensAsync(
        ApplicationUser user,
        string? remoteIpAddress,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiresAt) = await tokenService.CreateAccessTokenAsync(user);
        var (refreshToken, plainTextRefreshToken) = tokenService.CreateRefreshToken(user.Id, remoteIpAddress);

        dbContext.RefreshTokens.Add(refreshToken);
        RemoveExpiredRefreshTokens(user.Id);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            accessTokenExpiresAt,
            plainTextRefreshToken,
            refreshToken.ExpiresAt,
            MapUser(user, roles));
    }

    private void RemoveExpiredRefreshTokens(int userId)
    {
        var staleTokens = dbContext.RefreshTokens
            .Where(refreshToken =>
                refreshToken.ApplicationUserId == userId &&
                refreshToken.ExpiresAt <= DateTimeOffset.UtcNow.AddDays(-7))
            .ToList();

        if (staleTokens.Count > 0)
        {
            dbContext.RefreshTokens.RemoveRange(staleTokens);
        }
    }

    private static AuthenticatedUserResponse MapUser(ApplicationUser user, IEnumerable<string> roles) =>
        new(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.FullName,
            user.IsActive,
            roles.OrderBy(role => role).ToArray());
}