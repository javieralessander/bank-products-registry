using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(employee => employee.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(employee => employee.EmployeeCode)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(employee => employee.Department)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(employee => employee.EmployeeCode)
            .IsUnique();

        builder.HasIndex(employee => employee.Email)
            .IsUnique();
    }
}
