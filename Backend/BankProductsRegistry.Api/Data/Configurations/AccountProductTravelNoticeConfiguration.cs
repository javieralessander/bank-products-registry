using BankProductsRegistry.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankProductsRegistry.Api.Data.Configurations;

public sealed class AccountProductTravelNoticeConfiguration : IEntityTypeConfiguration<AccountProductTravelNotice>
{
    public void Configure(EntityTypeBuilder<AccountProductTravelNotice> builder)
    {
        builder.ToTable("AccountProductTravelNotices");
        builder.HasKey(travelNotice => travelNotice.Id);

        builder.Property(travelNotice => travelNotice.Reason)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(travelNotice => travelNotice.RequestedByUserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(travelNotice => travelNotice.CancelledByUserName)
            .HasMaxLength(100);

        builder.Property(travelNotice => travelNotice.CancellationReason)
            .HasMaxLength(300);

        builder.HasIndex(travelNotice => new { travelNotice.AccountProductId, travelNotice.StartsAt, travelNotice.EndsAt });

        builder.HasOne(travelNotice => travelNotice.AccountProduct)
            .WithMany(accountProduct => accountProduct.TravelNotices)
            .HasForeignKey(travelNotice => travelNotice.AccountProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
