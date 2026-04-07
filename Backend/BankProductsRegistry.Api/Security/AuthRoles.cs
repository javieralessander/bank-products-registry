namespace BankProductsRegistry.Api.Security;

public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operador";
    public const string ReadOnly = "Consulta";
    public const string Client = "Cliente";

    public static readonly string[] All = [Admin, Operator, ReadOnly, Client];
}
