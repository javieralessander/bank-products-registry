using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductAuditEntryConfiguration : IEntityTypeConfiguration<AccountProductAuditEntry>
{
    public void Configure(EntityTypeBuilder<AccountProductAuditEntry> builder)
    {
        builder.ToTable("AccountProductAuditEntries");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Action)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entry => entry.PerformedByUserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entry => entry.Detail)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(entry => new { entry.AccountProductId, entry.CreatedAt });

        builder.HasOne(entry => entry.AccountProduct)
            .WithMany(accountProduct => accountProduct.AuditEntries)
            .HasForeignKey(entry => entry.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entry => entry.AccountProductBlock)
            .WithMany()
            .HasForeignKey(entry => entry.AccountProductBlockId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
