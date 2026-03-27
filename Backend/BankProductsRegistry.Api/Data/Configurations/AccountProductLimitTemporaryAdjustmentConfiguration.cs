using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductLimitTemporaryAdjustmentConfiguration : IEntityTypeConfiguration<AccountProductLimitTemporaryAdjustment>
{
    public void Configure(EntityTypeBuilder<AccountProductLimitTemporaryAdjustment> builder)
    {
        builder.ToTable("AccountProductLimitTemporaryAdjustments");
        builder.HasKey(adjustment => adjustment.Id);

        builder.Property(adjustment => adjustment.CreditLimitTotal).HasPrecision(18, 2);
        builder.Property(adjustment => adjustment.DailyConsumptionLimit).HasPrecision(18, 2);
        builder.Property(adjustment => adjustment.PerTransactionLimit).HasPrecision(18, 2);
        builder.Property(adjustment => adjustment.AtmWithdrawalLimit).HasPrecision(18, 2);
        builder.Property(adjustment => adjustment.InternationalConsumptionLimit).HasPrecision(18, 2);
        builder.Property(adjustment => adjustment.Reason).HasMaxLength(300).IsRequired();
        builder.Property(adjustment => adjustment.ApprovedByUserName).HasMaxLength(100).IsRequired();

        builder.HasIndex(adjustment => new { adjustment.AccountProductId, adjustment.StartsAt, adjustment.EndsAt });

        builder.HasOne(adjustment => adjustment.AccountProduct)
            .WithMany(accountProduct => accountProduct.LimitAdjustments)
            .HasForeignKey(adjustment => adjustment.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
