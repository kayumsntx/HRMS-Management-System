namespace HRMS.Application.DTOs.Security;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = default!;
}

public class LoginHistoryFilterDto
{
    public string? UserName { get; set; }
    public bool? IsSuccess { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class LoginHistoryResponseDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string UserName { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public string? FailReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }      // UTC
    public DateTime? LogoutTime { get; set; }    // UTC
}

// ---- Menu ----
public class MenuCreateDto
{
    public int? ParentMenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public string? MenuUrl { get; set; }
    public string? IconClass { get; set; }
    public int SortOrder { get; set; }
}

public class MenuUpdateDto : MenuCreateDto
{
    public bool IsActive { get; set; } = true;
}

public class MenuResponseDto
{
    public int Id { get; set; }
    public int? ParentMenuId { get; set; }
    public string? ParentMenuName { get; set; }
    public string MenuName { get; set; } = default!;
    public string? MenuUrl { get; set; }
    public string? IconClass { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class MenuPermissionItemDto
{
    public int MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class MenuPermissionSaveDto
{
    public List<MenuPermissionItemDto> Items { get; set; } = new();
}

public class MenuPermissionResponseDto
{
    public int MenuId { get; set; }
    public int? ParentMenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public string? MenuUrl { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class MyMenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<MyMenuDto> Children { get; set; } = new();
}
