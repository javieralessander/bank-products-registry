namespace BankProductsRegistry.Api.Security;

/// <summary>
/// Roles delivered in the JWT. The internal <c>Employee</c> entity is the bank staff catalog;
/// it is not a separate app role. <see cref="Operator"/> matches the business term "ejecutivo"
/// (operational staff who manage products and clients).
/// </summary>
public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operador";
    public const string ReadOnly = "Consulta";
    public const string Client = "Cliente";

    public static readonly string[] All = [Admin, Operator, ReadOnly, Client];

    /// <summary>Admin, Operador y Consulta (personal interno; excluye Cliente).</summary>
    public const string InternalStaff = $"{Admin},{Operator},{ReadOnly}";
}
