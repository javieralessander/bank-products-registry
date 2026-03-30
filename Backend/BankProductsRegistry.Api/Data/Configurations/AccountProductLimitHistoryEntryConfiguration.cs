using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductLimitHistoryEntryConfiguration : IEntityTypeConfiguration<AccountProductLimitHistoryEntry>
{
    public void Configure(EntityTypeBuilder<AccountProductLimitHistoryEntry> builder)
    {
        builder.ToTable("AccountProductLimitHistoryEntries");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.ChangeType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entry => entry.PreviousCreditLimitTotal).HasPrecision(18, 2);
        builder.Property(entry => entry.NewCreditLimitTotal).HasPrecision(18, 2);
        builder.Property(entry => entry.PreviousDailyConsumptionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.NewDailyConsumptionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.PreviousPerTransactionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.NewPerTransactionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.PreviousAtmWithdrawalLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.NewAtmWithdrawalLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.PreviousInternationalConsumptionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.NewInternationalConsumptionLimit).HasPrecision(18, 2);
        builder.Property(entry => entry.Reason).HasMaxLength(300).IsRequired();
        builder.Property(entry => entry.PerformedByUserName).HasMaxLength(100).IsRequired();

        builder.HasIndex(entry => new { entry.AccountProductId, entry.CreatedAt });

        builder.HasOne(entry => entry.AccountProduct)
            .WithMany(accountProduct => accountProduct.LimitHistoryEntries)
            .HasForeignKey(entry => entry.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entry => entry.TemporaryAdjustment)
            .WithMany(adjustment => adjustment.HistoryEntries)
            .HasForeignKey(entry => entry.TemporaryAdjustmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
