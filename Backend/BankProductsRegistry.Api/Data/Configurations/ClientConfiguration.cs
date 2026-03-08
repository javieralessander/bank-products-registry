using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(client => client.Id);

        builder.Property(client => client.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(client => client.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(client => client.NationalId)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(client => client.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(client => client.Phone)
            .HasMaxLength(25)
            .IsRequired();

        builder.HasIndex(client => client.NationalId)
            .IsUnique();

        builder.HasIndex(client => client.Email)
            .IsUnique();
    }
}
