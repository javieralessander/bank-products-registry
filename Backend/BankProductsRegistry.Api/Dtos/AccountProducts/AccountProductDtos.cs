using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.AccountProducts;

public record AccountProductCreateRequest
{
    [Required]
    public int ClientId { get; init; }

    [Required]
    public int FinancialProductId { get; init; }

    [Required]
    public int EmployeeId { get; init; }

    [Required, MaxLength(30)]
    public string AccountNumber { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; init; }

    public DateTimeOffset OpenDate { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? MaturityDate { get; init; }

    public AccountProductStatus Status { get; init; } = AccountProductStatus.Active;
}

public sealed record AccountProductUpdateRequest : AccountProductCreateRequest;

public sealed record AccountProductPatchRequest
{
    public int? ClientId { get; init; }
    public int? FinancialProductId { get; init; }
    public int? EmployeeId { get; init; }

    [MaxLength(30)]
    public string? AccountNumber { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? Amount { get; init; }

    public DateTimeOffset? OpenDate { get; init; }
    public DateTimeOffset? MaturityDate { get; init; }
    public AccountProductStatus? Status { get; init; }
}

public sealed record AccountProductResponse(
    int Id,
    int ClientId,
    string ClientName,
    int FinancialProductId,
    string FinancialProductName,
    int EmployeeId,
    string EmployeeName,
    string AccountNumber,
    decimal Amount,
    DateTimeOffset OpenDate,
    DateTimeOffset? MaturityDate,
    AccountProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
