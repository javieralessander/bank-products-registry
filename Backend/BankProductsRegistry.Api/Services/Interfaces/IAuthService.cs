using BankProductsRegistry.Api.Dtos.Auth;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, string? remoteIpAddress, CancellationToken cancellationToken);
    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request, string? remoteIpAddress, CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(int userId, RevokeRefreshTokenRequest request, string? remoteIpAddress, CancellationToken cancellationToken);
    Task<AuthenticatedUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken);
}
