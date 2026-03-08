using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2);

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(300);

        builder.Property(transaction => transaction.ReferenceNumber)
            .HasMaxLength(60);

        builder.HasIndex(transaction => transaction.ReferenceNumber);

        builder.HasOne(transaction => transaction.AccountProduct)
            .WithMany(accountProduct => accountProduct.Transactions)
            .HasForeignKey(transaction => transaction.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
