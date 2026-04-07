using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Api.Dtos.Auth;

public sealed record LoginRequest
{
    [Required, MaxLength(150)]
    public string UserNameOrEmail { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}

// ---> ESTO ERA LO ÚNICO QUE TE FALTABA <---
public sealed record RegisterRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Apellido { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(25)]
    public string NationalId { get; init; } = string.Empty;

    [Required, MaxLength(25)]
    public string Phone { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}
// ------------------------------------------

public sealed record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed record RevokeRefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed record AuthenticatedUserResponse(
    int Id,
    string UserName,
    string Email,
    string FullName,
    bool IsActive,
    IReadOnlyCollection<string> Roles);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthenticatedUserResponse User);