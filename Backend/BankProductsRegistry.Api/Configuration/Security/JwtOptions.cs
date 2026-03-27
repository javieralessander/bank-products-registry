namespace BankProductsRegistry.Api.Configuration.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; init; } = "BankProductsRegistry";
    public string Audience { get; init; } = "BankProductsRegistry.Frontend";
    public string Key { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 20;
    public int RefreshTokenDays { get; init; } = 7;
}
