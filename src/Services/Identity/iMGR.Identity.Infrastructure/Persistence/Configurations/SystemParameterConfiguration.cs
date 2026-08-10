using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMGR.Identity.Infrastructure.Persistence.Configurations;

internal sealed class SystemParameterConfiguration : IEntityTypeConfiguration<SystemParameterRecord>
{
    public void Configure(EntityTypeBuilder<SystemParameterRecord> builder)
    {
        builder.ToTable("SystemParameter", "dbo");
        builder.HasKey(parameter => parameter.ParameterCode).HasName("PK_SystemParameter");
        builder.Property(parameter => parameter.ParameterCode).HasColumnName("ParameterCode").HasMaxLength(100);
        builder.Property(parameter => parameter.ParameterDescription).HasColumnName("ParameterDesc").HasMaxLength(200);
        builder.Property(parameter => parameter.ParameterValue).HasColumnName("ParameterValue").HasColumnType("ntext");
    }
}

