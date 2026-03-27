using BankProductsRegistry.Api.Models.Auth;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BankProductsRegistry.Api.Data;

public static class BankIdentitySeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        foreach (var roleName in AuthRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleCreation = await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
            EnsureIdentitySucceeded(roleCreation, $"No se pudo crear el rol {roleName}.");
        }

        var seedSection = configuration.GetSection("Authentication:SeedUsers");
        if (!seedSection.GetValue("Enabled", false))
        {
            return;
        }

        await EnsureUserAsync(userManager, seedSection.GetSection("Admin"), AuthRoles.Admin);
        await EnsureUserAsync(userManager, seedSection.GetSection("Operator"), AuthRoles.Operator);
        await EnsureUserAsync(userManager, seedSection.GetSection("ReadOnly"), AuthRoles.ReadOnly);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfigurationSection userSection,
        string roleName)
    {
        var userName = userSection["UserName"];
        var email = userSection["Email"];
        var fullName = userSection["FullName"];
        var password = userSection["Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var normalizedEmail = NormalizationHelper.NormalizeEmail(email);
        var normalizedName = NormalizationHelper.NormalizeName(fullName);

        var normalizedUserName = userName.Trim();
        var user = await userManager.FindByNameAsync(normalizedUserName) ?? await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedUserName,
                Email = normalizedEmail,
                FullName = normalizedName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureIdentitySucceeded(createResult, $"No se pudo crear el usuario semilla {userName}.");
        }
        else
        {
            user.UserName = normalizedUserName;
            user.Email = normalizedEmail;
            user.FullName = normalizedName;
            user.IsActive = true;
            user.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(user);
            EnsureIdentitySucceeded(updateResult, $"No se pudo actualizar el usuario semilla {userName}.");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            EnsureIdentitySucceeded(addRoleResult, $"No se pudo asignar el rol {roleName} al usuario {userName}.");
        }
    }

    private static void EnsureIdentitySucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {details}");
    }
}
