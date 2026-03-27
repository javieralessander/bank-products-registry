using System.ComponentModel.DataAnnotations;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Dtos.AccountProducts;

public sealed record AccountProductLimitUpsertRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? CreditLimitTotal { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? DailyConsumptionLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? PerTransactionLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? AtmWithdrawalLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? InternationalConsumptionLimit { get; init; }
}

public sealed record AccountProductLimitTemporaryAdjustmentCreateRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal? CreditLimitTotal { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? DailyConsumptionLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? PerTransactionLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? AtmWithdrawalLimit { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? InternationalConsumptionLimit { get; init; }

    public DateTimeOffset StartsAt { get; init; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset EndsAt { get; init; }

    [Required, MaxLength(300)]
    public string Reason { get; init; } = string.Empty;
}

public sealed record AccountProductLimitTemporaryAdjustmentSummaryResponse(
    int Id,
    decimal? CreditLimitTotal,
    decimal? DailyConsumptionLimit,
    decimal? PerTransactionLimit,
    decimal? AtmWithdrawalLimit,
    decimal? InternationalConsumptionLimit,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    int? ApprovedByUserId,
    string ApprovedByUserName);

public sealed record AccountProductLimitResponse(
    int AccountProductId,
    decimal? BaseCreditLimitTotal,
    decimal? BaseDailyConsumptionLimit,
    decimal? BasePerTransactionLimit,
    decimal? BaseAtmWithdrawalLimit,
    decimal? BaseInternationalConsumptionLimit,
    decimal? EffectiveCreditLimitTotal,
    decimal? EffectiveDailyConsumptionLimit,
    decimal? EffectivePerTransactionLimit,
    decimal? EffectiveAtmWithdrawalLimit,
    decimal? EffectiveInternationalConsumptionLimit,
    AccountProductLimitTemporaryAdjustmentSummaryResponse? ActiveTemporaryAdjustment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AccountProductLimitHistoryEntryResponse(
    int Id,
    int AccountProductId,
    int? TemporaryAdjustmentId,
    AccountProductLimitChangeType ChangeType,
    decimal? PreviousCreditLimitTotal,
    decimal? NewCreditLimitTotal,
    decimal? PreviousDailyConsumptionLimit,
    decimal? NewDailyConsumptionLimit,
    decimal? PreviousPerTransactionLimit,
    decimal? NewPerTransactionLimit,
    decimal? PreviousAtmWithdrawalLimit,
    decimal? NewAtmWithdrawalLimit,
    decimal? PreviousInternationalConsumptionLimit,
    decimal? NewInternationalConsumptionLimit,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string Reason,
    int? PerformedByUserId,
    string PerformedByUserName,
    DateTimeOffset CreatedAt);
