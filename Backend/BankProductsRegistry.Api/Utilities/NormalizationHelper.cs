namespace BankProductsRegistry.Api.Utilities;

public static class NormalizationHelper
{
    public static string NormalizeName(string value) => value.Trim();

    public static string NormalizeEmail(string value) => value.Trim().ToLowerInvariant();

    public static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();

    public static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
