using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankProductsRegistry.Api.Configuration.Security;
using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BankProductsRegistry.Api.Services.Auth;

public sealed class TokenService(
    IOptions<JwtOptions> jwtOptions,
    UserManager<ApplicationUser> userManager) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<(string Token, DateTimeOffset ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim("full_name", user.FullName));
        }

        if (!string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            claims.Add(new Claim(CustomClaimTypes.SecurityStamp, user.SecurityStamp));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwtToken), expiresAt);
    }

    public (RefreshToken RefreshToken, string PlainTextToken) CreateRefreshToken(int userId, string? createdByIp)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        return (
            new RefreshToken
            {
                ApplicationUserId = userId,
                TokenHash = HashRefreshToken(rawToken),
                ExpiresAt = expiresAt,
                CreatedByIp = NormalizationHelper.NormalizeOptionalText(createdByIp)
            },
            rawToken);
    }

    public string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(bytes);
    }
}
