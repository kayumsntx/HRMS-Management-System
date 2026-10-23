using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> b)
    {
        b.ToTable("LoginHistories");
        b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        b.Property(x => x.FailReason).HasMaxLength(200);
        b.Property(x => x.IpAddress).HasMaxLength(50);
        b.Property(x => x.UserAgent).HasMaxLength(500);
        b.HasIndex(x => new { x.UserName, x.LoginTime });
        b.HasIndex(x => x.UserId);
        b.RestrictFk<LoginHistory, User>(x => x.UserId);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.ReplacedByHash).HasMaxLength(64);
        b.Property(x => x.CreatedByIp).HasMaxLength(50);
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.UserId);
        b.RestrictFk<RefreshToken, User>(x => x.UserId);
    }
}

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> b)
    {
        b.ToTable("Menus");
        b.Property(x => x.MenuName).HasMaxLength(100).IsRequired();
        b.Property(x => x.MenuUrl).HasMaxLength(200);
        b.Property(x => x.IconClass).HasMaxLength(100);
        b.HasIndex(x => new { x.ParentMenuId, x.MenuName }).IsUnique();
        b.RestrictFk<Menu, Menu>(x => x.ParentMenuId);
    }
}

public class MenuPermissionConfiguration : IEntityTypeConfiguration<MenuPermission>
{
    public void Configure(EntityTypeBuilder<MenuPermission> b)
    {
        b.ToTable("MenuPermissions");
        b.HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();
        b.RestrictFk<MenuPermission, Role>(x => x.RoleId);
        b.RestrictFk<MenuPermission, Menu>(x => x.MenuId);
    }
}
