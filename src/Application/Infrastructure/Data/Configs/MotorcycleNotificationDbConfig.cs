using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class MotorcycleNotificationDbConfig : IEntityTypeConfiguration<MotorcycleNotification>
{
    public void Configure(EntityTypeBuilder<MotorcycleNotification> builder)
    {
        builder.HasKey(x => x.Id);
    }
}