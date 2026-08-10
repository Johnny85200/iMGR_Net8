using IMGR.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMGR.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "dbo");
        builder.HasKey(user => user.UserId).HasName("PK_Users");

        builder.Property(user => user.UserId).HasColumnName("UserID").ValueGeneratedOnAdd();
        builder.Property(user => user.LoginId).HasColumnName("LoginID").HasMaxLength(20);
        builder.Property(user => user.UserName).HasColumnName("UserName").HasMaxLength(100);
        builder.Property(user => user.UserEmail).HasColumnName("UserEmail").HasMaxLength(50);
        builder.Property(user => user.UserMobileNo).HasColumnName("UserMobileNo").HasMaxLength(20);
        builder.Property(user => user.PasswordHash).HasColumnName("UserPassword").HasMaxLength(255);
        builder.Property(user => user.AccountStatus).HasColumnName("UserAccountStatus").HasMaxLength(1);
        builder.Property(user => user.ExpiryDate).HasColumnName("ExpiryDate").HasColumnType("datetime");
        builder.Property(user => user.UserChangePassword).HasColumnName("UserChangePassword");
        builder.Property(user => user.UserChangePasswordPeriod).HasColumnName("UserChangePasswordPeriod");
        builder.Property(user => user.UserChangePasswordUnit).HasColumnName("UserChangePasswordUnit").HasMaxLength(1);
        builder.Property(user => user.UserChangePasswordDate).HasColumnName("UserChangePasswordDate").HasColumnType("datetime");
        builder.Property(user => user.FailCount).HasColumnName("FailCount");
        builder.Property(user => user.UserLanguage).HasColumnName("UserLanguage").HasMaxLength(10);
        builder.Property(user => user.UserIsKeepConnected).HasColumnName("UserIsKeepConnected");
        builder.Property(user => user.UsersCannotCreateUsersWithMorePermission)
            .HasColumnName("UsersCannotCreateUsersWithMorePermission");
    }
}

