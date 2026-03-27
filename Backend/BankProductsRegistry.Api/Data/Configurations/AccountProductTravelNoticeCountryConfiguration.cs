using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductTravelNoticeCountryConfiguration : IEntityTypeConfiguration<AccountProductTravelNoticeCountry>
{
    public void Configure(EntityTypeBuilder<AccountProductTravelNoticeCountry> builder)
    {
        builder.ToTable("AccountProductTravelNoticeCountries");
        builder.HasKey(country => country.Id);

        builder.Property(country => country.CountryCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasIndex(country => new { country.TravelNoticeId, country.CountryCode })
            .IsUnique();

        builder.HasOne(country => country.TravelNotice)
            .WithMany(travelNotice => travelNotice.Countries)
            .HasForeignKey(country => country.TravelNoticeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
