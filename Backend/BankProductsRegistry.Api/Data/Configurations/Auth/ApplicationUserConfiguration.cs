using BankProductsRegistry.Api.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations.Auth;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(token => token.User)
            .HasForeignKey(token => token.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
