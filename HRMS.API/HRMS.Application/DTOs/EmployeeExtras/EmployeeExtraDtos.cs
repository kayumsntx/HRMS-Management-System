using HRMS.Application.DTOs.Common;

namespace HRMS.Application.DTOs.EmployeeExtras;

// ---------------- Skill ----------------
public abstract class EmployeeSkillBase
{
    public int SkillCategoryId { get; set; }
    public string SkillName { get; set; } = default!;
    public string? SkillLevel { get; set; }      // Beginner, Intermediate, Advanced, Expert
    public string? Remarks { get; set; }
}
public class EmployeeSkillCreateDto : EmployeeSkillBase, IHasEmployeeId { public int EmployeeId { get; set; } }
public class EmployeeSkillUpdateDto : EmployeeSkillBase { public bool IsActive { get; set; } = true; }
public class EmployeeSkillResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SkillCategoryId { get; set; }
    public string? SkillCategoryName { get; set; }
    public string SkillName { get; set; } = default!;
    public string? SkillLevel { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
}

// ---------------- Previous job ----------------
public abstract class EmployeePreviousJobBase
{
    public string CompanyName { get; set; } = default!;
    public string? Designation { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? LastSalary { get; set; }
    public string? LeavingReason { get; set; }
}
public class EmployeePreviousJobCreateDto : EmployeePreviousJobBase, IHasEmployeeId { public int EmployeeId { get; set; } }
public class EmployeePreviousJobUpdateDto : EmployeePreviousJobBase { public bool IsActive { get; set; } = true; }
public class EmployeePreviousJobResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string CompanyName { get; set; } = default!;
    public string? Designation { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? LastSalary { get; set; }
    public string? LeavingReason { get; set; }
    public bool IsActive { get; set; }
}

// ---------------- Address ----------------
public abstract class EmployeeAddressBase
{
    public string AddressType { get; set; } = default!;   // Present / Permanent
    public string? Village { get; set; }
    public string? PostOffice { get; set; }
    public string? PoliceStation { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? FullAddress { get; set; }
}
public class EmployeeAddressCreateDto : EmployeeAddressBase, IHasEmployeeId { public int EmployeeId { get; set; } }
public class EmployeeAddressUpdateDto : EmployeeAddressBase { public bool IsActive { get; set; } = true; }
public class EmployeeAddressResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string AddressType { get; set; } = default!;
    public string? Village { get; set; }
    public string? PostOffice { get; set; }
    public string? PoliceStation { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? FullAddress { get; set; }
    public bool IsActive { get; set; }
}

// ---------------- Bank account ----------------
public abstract class EmployeeBankAccountBase
{
    public int PaymentTypeId { get; set; }
    public int? BankId { get; set; }
    public string? BranchName { get; set; }
    public string AccountNumber { get; set; } = default!;
}
public class EmployeeBankAccountCreateDto : EmployeeBankAccountBase, IHasEmployeeId { public int EmployeeId { get; set; } }
public class EmployeeBankAccountUpdateDto : EmployeeBankAccountBase { public bool IsActive { get; set; } = true; }
public class EmployeeBankAccountResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }
    public int? BankId { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string AccountNumber { get; set; } = default!;
    public bool IsActive { get; set; }
}
