namespace HRMS.Application.DTOs.UserRoles;

public class UserRoleResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? AssignedBy { get; set; }
}
