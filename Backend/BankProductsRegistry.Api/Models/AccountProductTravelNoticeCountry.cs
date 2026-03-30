namespace BankProductsRegistry.Api.Models;

public sealed class AccountProductTravelNoticeCountry : BaseEntity
{
    public int TravelNoticeId { get; set; }
    public string CountryCode { get; set; } = string.Empty;

    public AccountProductTravelNotice? TravelNotice { get; set; }
}
