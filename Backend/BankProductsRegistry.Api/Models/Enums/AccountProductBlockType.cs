using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum AccountProductBlockType
{
    [JsonStringEnumMemberName("temporal")]
    Temporary = 1,
    [JsonStringEnumMemberName("permanente")]
    Permanent = 2,
    [JsonStringEnumMemberName("fraude")]
    Fraud = 3
}
