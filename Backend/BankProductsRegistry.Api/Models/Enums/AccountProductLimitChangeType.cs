using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum AccountProductLimitChangeType
{
    [JsonStringEnumMemberName("configuracion_inicial")]
    InitialConfiguration = 1,
    [JsonStringEnumMemberName("actualizacion_base")]
    BaseLimitUpdated = 2,
    [JsonStringEnumMemberName("ajuste_temporal")]
    TemporaryAdjustmentScheduled = 3
}
