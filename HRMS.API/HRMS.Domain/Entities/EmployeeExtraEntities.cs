using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class EmployeeSkill : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public int SkillCategoryId { get; set; }
    public string SkillName { get; set; } = default!;
    public string? SkillLevel { get; set; }
    public string? Remarks { get; set; }
}

public class EmployeePreviousJob : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public string CompanyName { get; set; } = default!;
    public string? Designation { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? LastSalary { get; set; }
    public string? LeavingReason { get; set; }
}

public class EmployeeAddress : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public string AddressType { get; set; } = default!;   // Present / Permanent
    public string? Village { get; set; }
    public string? PostOffice { get; set; }
    public string? PoliceStation { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? FullAddress { get; set; }
}

public class EmployeeBankAccount : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public int PaymentTypeId { get; set; }
    public int? BankId { get; set; }
    public string? BranchName { get; set; }
    public string AccountNumber { get; set; } = default!;
}
