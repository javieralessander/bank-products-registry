using BankProductsRegistry.Api.Models.Auth;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface ITokenService
{
    Task<(string Token, DateTimeOffset ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user);
    (RefreshToken RefreshToken, string PlainTextToken) CreateRefreshToken(int userId, string? createdByIp);
    string HashRefreshToken(string token);
}
