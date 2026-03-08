using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Api.Dtos.Clients;

public record ClientCreateRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, MaxLength(25)]
    public string NationalId { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(25)]
    public string Phone { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed record ClientUpdateRequest : ClientCreateRequest;

public sealed record ClientPatchRequest
{
    [MaxLength(100)]
    public string? FirstName { get; init; }

    [MaxLength(100)]
    public string? LastName { get; init; }

    [MaxLength(25)]
    public string? NationalId { get; init; }

    [EmailAddress, MaxLength(150)]
    public string? Email { get; init; }

    [MaxLength(25)]
    public string? Phone { get; init; }

    public bool? IsActive { get; init; }
}

public sealed record ClientResponse(
    int Id,
    string FirstName,
    string LastName,
    string NationalId,
    string Email,
    string Phone,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
