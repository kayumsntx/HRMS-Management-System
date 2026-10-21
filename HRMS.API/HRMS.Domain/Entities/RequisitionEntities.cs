using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class RequisitionType : BaseEntity
{
    public string TypeName { get; set; } = default!;
    public string? Description { get; set; }
}

public class Requisition : BaseEntity
{
    public string RequisitionNo { get; set; } = default!;
    public int RequisitionTypeId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime RequisitionDate { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = "Pending";
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public List<RequisitionDetail> Details { get; set; } = new();
}

public class RequisitionDetail : BaseEntity
{
    public int RequisitionId { get; set; }
    public string ItemName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Remarks { get; set; }
}
