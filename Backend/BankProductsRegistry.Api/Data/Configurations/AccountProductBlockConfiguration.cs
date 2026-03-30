using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductBlockConfiguration : IEntityTypeConfiguration<AccountProductBlock>
{
    public void Configure(EntityTypeBuilder<AccountProductBlock> builder)
    {
        builder.ToTable("AccountProductBlocks");
        builder.HasKey(block => block.Id);

        builder.Property(block => block.BlockType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(block => block.Reason)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(block => block.AppliedByUserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(block => block.ReleasedByUserName)
            .HasMaxLength(100);

        builder.Property(block => block.ReleaseReason)
            .HasMaxLength(300);

        builder.HasIndex(block => new { block.AccountProductId, block.ReleasedAt, block.StartsAt });

        builder.HasOne(block => block.AccountProduct)
            .WithMany(accountProduct => accountProduct.Blocks)
            .HasForeignKey(block => block.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
