using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum AccountProductAuditAction
{
    [JsonStringEnumMemberName("bloqueo_aplicado")]
    BlockApplied = 1,
    [JsonStringEnumMemberName("bloqueo_liberado")]
    BlockReleased = 2,
    [JsonStringEnumMemberName("bloqueo_expirado")]
    BlockExpired = 3
}
