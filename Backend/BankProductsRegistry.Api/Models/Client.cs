namespace BankProductsRegistry.Api.Models;

public sealed class Client : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Auth.ApplicationUser? User { get; set; }

    public ICollection<AccountProduct> AccountProducts { get; set; } = new List<AccountProduct>();
}
