using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class MotorcycleDbConfig : IEntityTypeConfiguration<Motorcycle>
{
    public void Configure(EntityTypeBuilder<Motorcycle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Identifier).IsUnique();
        builder.HasIndex(x => x.LicensePlate).IsUnique();

        builder.Property(x => x.Identifier)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Year)
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LicensePlate)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasMany(x => x.Rentals)
            .WithOne(x => x.Motorcycle)
            .HasForeignKey(x => x.MotorcycleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}