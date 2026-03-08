using System.Text.Json.Serialization;

namespace BankProductsRegistry.Api.Models.Enums;

public enum ProductType
{
    [JsonStringEnumMemberName("tarjeta_credito")]
    CreditCard = 1,
    [JsonStringEnumMemberName("prestamo")]
    Loan = 2,
    [JsonStringEnumMemberName("inversion")]
    Investment = 3,
    [JsonStringEnumMemberName("certificado")]
    Certificate = 4,
    [JsonStringEnumMemberName("cuenta_ahorro")]
    SavingsAccount = 5
}
