using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RentalPlanDbConfig : IEntityTypeConfiguration<RentalPlan>
{
    public void Configure(EntityTypeBuilder<RentalPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DailyAmount)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.PenaltyPercentage)
            .HasColumnType("decimal(5,2)");

        builder.HasMany(x => x.Rentals)
            .WithOne(x => x.RentalPlan)
            .HasForeignKey(x => x.RentalPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new List<RentalPlan>
        {
            RentalPlan.Create(Guid.Parse("0199d9bc-f73b-74a7-9843-b6c79c2d40c6"), 7, 30, 20).Value,
            RentalPlan.Create(Guid.Parse("0199d9bc-f73b-72de-8e2f-d18545ceca6f"), 15, 28, 40).Value,
            RentalPlan.Create(Guid.Parse("0199d9bc-f73b-7e14-b426-61fc9704d44d"), 30, 22, 0).Value,
            RentalPlan.Create(Guid.Parse("0199d9bc-f73b-7d14-895c-bb90b96fe378"), 45, 20, 0).Value,
            RentalPlan.Create(Guid.Parse("0199d9bc-f73b-70bf-9e79-473208dc909e"), 50, 18, 0).Value
        });
    }
}