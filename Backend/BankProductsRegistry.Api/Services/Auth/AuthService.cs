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
