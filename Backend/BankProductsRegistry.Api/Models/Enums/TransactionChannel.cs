using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum TransactionChannel
{
    [JsonStringEnumMemberName("sucursal")]
    Branch = 1,
    [JsonStringEnumMemberName("atm")]
    Atm = 2,
    [JsonStringEnumMemberName("pos")]
    PointOfSale = 3,
    [JsonStringEnumMemberName("en_linea")]
    Online = 4,
    [JsonStringEnumMemberName("transferencia")]
    Transfer = 5,
    [JsonStringEnumMemberName("backoffice")]
    BackOffice = 6
}
