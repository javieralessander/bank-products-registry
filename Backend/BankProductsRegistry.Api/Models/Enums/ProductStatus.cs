using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum ProductStatus
{
    [JsonStringEnumMemberName("borrador")]
    Draft = 1,
    [JsonStringEnumMemberName("activo")]
    Active = 2,
    [JsonStringEnumMemberName("suspendido")]
    Suspended = 3,
    [JsonStringEnumMemberName("cerrado")]
    Closed = 4
}
