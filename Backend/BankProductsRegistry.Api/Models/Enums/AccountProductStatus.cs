using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum AccountProductStatus
{
    [JsonStringEnumMemberName("pendiente")]
    Pending = 1,
    [JsonStringEnumMemberName("activo")]
    Active = 2,
    [JsonStringEnumMemberName("en_mora")]
    Delinquent = 3,
    [JsonStringEnumMemberName("cerrado")]
    Closed = 4,
    [JsonStringEnumMemberName("cancelado")]
    Cancelled = 5
}
