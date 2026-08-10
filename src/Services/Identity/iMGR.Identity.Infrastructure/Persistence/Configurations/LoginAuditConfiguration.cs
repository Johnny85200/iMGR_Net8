using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMGR.Identity.Infrastructure.Persistence.Configurations;

internal sealed class LoginAuditConfiguration : IEntityTypeConfiguration<LoginAuditRecord>
{
    public void Configure(EntityTypeBuilder<LoginAuditRecord> builder)
    {
        builder.ToTable("LoginAudit", "dbo");
        builder.HasKey(audit => audit.LoginAuditId).HasName("PK_LoginAudit");

        builder.Property(audit => audit.LoginAuditId).HasColumnName("LoginAuditID").ValueGeneratedOnAdd();
        builder.Property(audit => audit.UserId).HasColumnName("UserID");
        builder.Property(audit => audit.LoginId).HasColumnName("LoginAuditLoginID").HasMaxLength(255);
        builder.Property(audit => audit.LoginMachine).HasColumnName("LoginAuditLoginMachine").HasMaxLength(255);
        builder.Property(audit => audit.LoginIpAddress).HasColumnName("LoginAuditLoginIPAddress").HasMaxLength(255);
        builder.Property(audit => audit.LoginAgent).HasColumnName("LoginAuditLoginAgent").HasColumnType("ntext");
        builder.Property(audit => audit.LoginDateTime).HasColumnName("LoginAuditLoginDateTime").HasColumnType("datetime");
        builder.Property(audit => audit.IsLoginFail).HasColumnName("LoginAuditIsLoginFail");
        builder.Property(audit => audit.LoginErrorMessage).HasColumnName("LoginAuditLoginErrorMesage").HasMaxLength(255);
    }
}

