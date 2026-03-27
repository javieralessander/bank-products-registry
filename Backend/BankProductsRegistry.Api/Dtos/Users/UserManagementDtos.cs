using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Api.Dtos.Users;

public sealed record UserCreateRequest
{
    [Required, MaxLength(100)]
    public string UserName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed record UserStatusUpdateRequest
{
    public bool IsActive { get; init; }
}

public sealed record UserRoleUpdateRequest
{
    [Required, MaxLength(20)]
    public string Role { get; init; } = string.Empty;
}

public sealed record UserResetPasswordRequest
{
    [Required, MinLength(8), DataType(DataType.Password)]
    public string NewPassword { get; init; } = string.Empty;
}

public sealed record UserManagementResponse(
    int Id,
    string UserName,
    string Email,
    string FullName,
    bool IsActive,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles);
