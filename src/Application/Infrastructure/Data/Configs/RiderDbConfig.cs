using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configs;

public class RiderDbConfig : IEntityTypeConfiguration<Rider>
{
    public void Configure(EntityTypeBuilder<Rider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Cnpj)
            .IsUnique();

        builder.HasIndex(x => x.Cnh)
            .IsUnique();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Rider_CnhType",
            "cnh_type IN ('A', 'B', 'A+B')"));
    }
}