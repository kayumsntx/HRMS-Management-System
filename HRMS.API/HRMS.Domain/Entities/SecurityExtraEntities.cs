using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class LoginHistory : BaseEntity
{
    public int? UserId { get; set; }
    public string UserName { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public string? FailReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;   // UTC
    public DateTime? LogoutTime { get; set; }
}

// Only the SHA-256 hash of the refresh token is stored, never the token itself.
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByHash { get; set; }
}

public class Menu : BaseEntity
{
    public int? ParentMenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public string? MenuUrl { get; set; }
    public string? IconClass { get; set; }
    public int SortOrder { get; set; }
}

public class MenuPermission : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
