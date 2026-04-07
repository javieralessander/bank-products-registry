using Microsoft.AspNetCore.Identity;
using BankProductsRegistry.Api.Models;

namespace BankProductsRegistry.Api.Models.Auth;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}
