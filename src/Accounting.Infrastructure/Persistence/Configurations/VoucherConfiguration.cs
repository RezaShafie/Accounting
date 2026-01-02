using Accounting.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VoucherNumber)
            .IsRequired();

        builder.Property(v => v.Description)
            .HasMaxLength(500);

        builder.Property(v => v.Date)
            .HasConversion(
                dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                dateTime => DateOnly.FromDateTime(dateTime)
            )
            .HasColumnType("date");

        builder.HasMany(v => v.Lines)
            .WithOne()
            .HasForeignKey(l => l.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}