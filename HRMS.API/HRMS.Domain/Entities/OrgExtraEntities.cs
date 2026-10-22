using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class SubSection : BaseEntity
{
    public int SectionId { get; set; }
    public string SubSectionName { get; set; } = default!;
}

public class JobLocation : BaseEntity
{
    public string LocationName { get; set; } = default!;
    public string? Address { get; set; }
}

public class StaffCategory : BaseEntity
{
    public string CategoryName { get; set; } = default!;
}

public class SkillCategory : BaseEntity
{
    public string CategoryName { get; set; } = default!;
}

public class PaymentType : BaseEntity
{
    public string PaymentTypeName { get; set; } = default!;
}

public class Bank : BaseEntity
{
    public string BankName { get; set; } = default!;
}
