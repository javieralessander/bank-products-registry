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

        builder.Property(user => user.FirstName)
            .HasMaxLength(100);

        builder.Property(user => user.LastName)
            .HasMaxLength(100);

        builder.Property(user => user.NationalId)
            .HasMaxLength(25);

        builder.Property(user => user.Phone)
            .HasMaxLength(25);

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(user => user.Client)
            .WithOne(client => client.User)
            .HasForeignKey<ApplicationUser>(user => user.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(user => user.ClientId)
            .IsUnique();

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(token => token.User)
            .HasForeignKey(token => token.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
