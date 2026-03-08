using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum TransactionType
{
    [JsonStringEnumMemberName("deposito")]
    Deposit = 1,
    [JsonStringEnumMemberName("retiro")]
    Withdrawal = 2,
    [JsonStringEnumMemberName("pago")]
    Payment = 3,
    [JsonStringEnumMemberName("transferencia")]
    Transfer = 4,
    [JsonStringEnumMemberName("cargo")]
    Fee = 5
}
