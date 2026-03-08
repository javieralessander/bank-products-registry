using System.ComponentModel.DataAnnotations;

namespace BankProductsRegistry.Api.Dtos.Employees;

public record EmployeeCreateRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, MaxLength(30)]
    public string EmployeeCode { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Department { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed record EmployeeUpdateRequest : EmployeeCreateRequest;

public sealed record EmployeePatchRequest
{
    [MaxLength(100)]
    public string? FirstName { get; init; }

    [MaxLength(100)]
    public string? LastName { get; init; }

    [MaxLength(30)]
    public string? EmployeeCode { get; init; }

    [EmailAddress, MaxLength(150)]
    public string? Email { get; init; }

    [MaxLength(100)]
    public string? Department { get; init; }

    public bool? IsActive { get; init; }
}

public sealed record EmployeeResponse(
    int Id,
    string FirstName,
    string LastName,
    string EmployeeCode,
    string Email,
    string Department,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
