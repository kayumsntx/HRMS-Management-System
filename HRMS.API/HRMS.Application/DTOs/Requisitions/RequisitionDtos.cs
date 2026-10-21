namespace HRMS.Application.DTOs.Requisitions;

public class RequisitionItemDto
{
    public string ItemName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Remarks { get; set; }
}

public class RequisitionCreateDto
{
    public int EmployeeId { get; set; }
    public int RequisitionTypeId { get; set; }
    public DateTime? RequisitionDate { get; set; }
    public string? Purpose { get; set; }
    public List<RequisitionItemDto> Items { get; set; } = new();
}

public class MyRequisitionCreateDto
{
    public int RequisitionTypeId { get; set; }
    public DateTime? RequisitionDate { get; set; }
    public string? Purpose { get; set; }
    public List<RequisitionItemDto> Items { get; set; } = new();
}

public class RequisitionItemResponseDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Remarks { get; set; }
}

public class RequisitionResponseDto
{
    public int Id { get; set; }
    public string RequisitionNo { get; set; } = default!;
    public int RequisitionTypeId { get; set; }
    public string? TypeName { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime RequisitionDate { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<RequisitionItemResponseDto> Items { get; set; } = new();
}
