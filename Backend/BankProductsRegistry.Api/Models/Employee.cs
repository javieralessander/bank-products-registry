namespace BankProductsRegistry.Api.Models;

public sealed class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<AccountProduct> ManagedProducts { get; set; } = new List<AccountProduct>();
}
