using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductLimitConfiguration : IEntityTypeConfiguration<AccountProductLimit>
{
    public void Configure(EntityTypeBuilder<AccountProductLimit> builder)
    {
        builder.ToTable("AccountProductLimits");
        builder.HasKey(limit => limit.Id);

        builder.Property(limit => limit.CreditLimitTotal).HasPrecision(18, 2);
        builder.Property(limit => limit.DailyConsumptionLimit).HasPrecision(18, 2);
        builder.Property(limit => limit.PerTransactionLimit).HasPrecision(18, 2);
        builder.Property(limit => limit.AtmWithdrawalLimit).HasPrecision(18, 2);
        builder.Property(limit => limit.InternationalConsumptionLimit).HasPrecision(18, 2);

        builder.HasIndex(limit => limit.AccountProductId).IsUnique();

        builder.HasOne(limit => limit.AccountProduct)
            .WithOne(accountProduct => accountProduct.LimitProfile)
            .HasForeignKey<AccountProductLimit>(limit => limit.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
