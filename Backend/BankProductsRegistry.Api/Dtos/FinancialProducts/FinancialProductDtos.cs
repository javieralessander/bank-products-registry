using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.FinancialProducts;

public record FinancialProductCreateRequest
{
    [Required, MaxLength(120)]
    public string ProductName { get; init; } = string.Empty;

    [Required]
    public ProductType ProductType { get; init; }

    [Range(0, 100)]
    public decimal InterestRate { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public ProductStatus Status { get; init; } = ProductStatus.Active;

    [Required, MaxLength(3), MinLength(3)]
    public string Currency { get; init; } = "DOP";

    [Range(0, double.MaxValue)]
    public decimal MinimumOpeningAmount { get; init; }
}

public sealed record FinancialProductUpdateRequest : FinancialProductCreateRequest;

public sealed record FinancialProductPatchRequest
{
    [MaxLength(120)]
    public string? ProductName { get; init; }

    public ProductType? ProductType { get; init; }

    [Range(0, 100)]
    public decimal? InterestRate { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public ProductStatus? Status { get; init; }

    [MaxLength(3), MinLength(3)]
    public string? Currency { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumOpeningAmount { get; init; }
}

public sealed record FinancialProductResponse(
    int Id,
    string ProductName,
    ProductType ProductType,
    decimal InterestRate,
    string? Description,
    ProductStatus Status,
    string Currency,
    decimal MinimumOpeningAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
