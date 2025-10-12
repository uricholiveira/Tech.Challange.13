using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RentalDbConfig : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpectedAmount)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.PenaltyAmount)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(12,2)");

        builder.Navigation(x => x.RentalPlan).AutoInclude();
    }
}