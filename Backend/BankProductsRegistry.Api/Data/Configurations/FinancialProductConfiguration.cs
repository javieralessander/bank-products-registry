using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class FinancialProductConfiguration : IEntityTypeConfiguration<FinancialProduct>
{
    public void Configure(EntityTypeBuilder<FinancialProduct> builder)
    {
        builder.ToTable("FinancialProducts");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.ProductName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(product => product.ProductType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(500);

        builder.Property(product => product.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(product => product.InterestRate)
            .HasPrecision(9, 4);

        builder.Property(product => product.MinimumOpeningAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(product => new { product.ProductName, product.ProductType });
    }
}
