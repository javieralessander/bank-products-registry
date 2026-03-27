namespace BankProductsRegistry.Api.Security;

public static class AuthPolicies
{
    public const string WriteAccess = nameof(WriteAccess);
    public const string AdminOnly = nameof(AdminOnly);
}
