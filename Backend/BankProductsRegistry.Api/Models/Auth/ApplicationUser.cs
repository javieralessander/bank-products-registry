using Microsoft.AspNetCore.Identity;

namespace BankProductsRegistry.Api.Models.Auth;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}
