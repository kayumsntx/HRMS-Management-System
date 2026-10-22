namespace HRMS.Application.DTOs.Masters;

public class SubSectionCreateDto { public int SectionId { get; set; } public string SubSectionName { get; set; } = default!; }
public class SubSectionUpdateDto : SubSectionCreateDto { public bool IsActive { get; set; } = true; }
public class SubSectionResponseDto { public int Id { get; set; } public int SectionId { get; set; } public string? SectionName { get; set; } public string SubSectionName { get; set; } = default!; public bool IsActive { get; set; } }

public class JobLocationCreateDto { public string LocationName { get; set; } = default!; public string? Address { get; set; } }
public class JobLocationUpdateDto : JobLocationCreateDto { public bool IsActive { get; set; } = true; }
public class JobLocationResponseDto { public int Id { get; set; } public string LocationName { get; set; } = default!; public string? Address { get; set; } public bool IsActive { get; set; } }

public class StaffCategoryCreateDto { public string CategoryName { get; set; } = default!; }
public class StaffCategoryUpdateDto : StaffCategoryCreateDto { public bool IsActive { get; set; } = true; }
public class StaffCategoryResponseDto { public int Id { get; set; } public string CategoryName { get; set; } = default!; public bool IsActive { get; set; } }

public class SkillCategoryCreateDto { public string CategoryName { get; set; } = default!; }
public class SkillCategoryUpdateDto : SkillCategoryCreateDto { public bool IsActive { get; set; } = true; }
public class SkillCategoryResponseDto { public int Id { get; set; } public string CategoryName { get; set; } = default!; public bool IsActive { get; set; } }

public class PaymentTypeCreateDto { public string PaymentTypeName { get; set; } = default!; }
public class PaymentTypeUpdateDto : PaymentTypeCreateDto { public bool IsActive { get; set; } = true; }
public class PaymentTypeResponseDto { public int Id { get; set; } public string PaymentTypeName { get; set; } = default!; public bool IsActive { get; set; } }

public class BankCreateDto { public string BankName { get; set; } = default!; }
public class BankUpdateDto : BankCreateDto { public bool IsActive { get; set; } = true; }
public class BankResponseDto { public int Id { get; set; } public string BankName { get; set; } = default!; public bool IsActive { get; set; } }
