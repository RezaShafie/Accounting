// File: Accounting.Infrastructure/Persistence/Configurations/VoucherLineConfiguration.cs

using System.Text.Json;
using Accounting.Domain.Entities;
using Accounting.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounting.Infrastructure.Persistence.Configurations;

public class VoucherLineConfiguration : IEntityTypeConfiguration<VoucherLine>
{
    public void Configure(EntityTypeBuilder<VoucherLine> builder)
    {
        builder.ToTable("VoucherLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Debit)
            .HasColumnType("decimal(18,2)");

        builder.Property(l => l.Credit)
            .HasColumnType("decimal(18,2)");


        builder.Property(l => l.AccountCoding)
            .HasConversion(
                obj => JsonSerializer.Serialize(obj.Segments, (JsonSerializerOptions?)null),
                jsonString => AccountCoding.CreateFromSegments(
                    JsonSerializer.Deserialize<string[]>(jsonString, (JsonSerializerOptions?)null) ?? Array.Empty<string>()
                )
            )
            .HasColumnType("nvarchar(max)")
            .IsRequired();
    }
}