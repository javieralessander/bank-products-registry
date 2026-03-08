using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductConfiguration : IEntityTypeConfiguration<AccountProduct>
{
    public void Configure(EntityTypeBuilder<AccountProduct> builder)
    {
        builder.ToTable("AccountProducts");
        builder.HasKey(accountProduct => accountProduct.Id);

        builder.Property(accountProduct => accountProduct.AccountNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(accountProduct => accountProduct.Amount)
            .HasPrecision(18, 2);

        builder.Property(accountProduct => accountProduct.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(accountProduct => accountProduct.AccountNumber)
            .IsUnique();

        builder.HasOne(accountProduct => accountProduct.Client)
            .WithMany(client => client.AccountProducts)
            .HasForeignKey(accountProduct => accountProduct.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(accountProduct => accountProduct.FinancialProduct)
            .WithMany(product => product.AccountProducts)
            .HasForeignKey(accountProduct => accountProduct.FinancialProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(accountProduct => accountProduct.Employee)
            .WithMany(employee => employee.ManagedProducts)
            .HasForeignKey(accountProduct => accountProduct.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
