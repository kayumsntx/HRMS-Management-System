using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Requisitions;

namespace HRMS.Application.Interfaces.Services;

public interface IRequisitionService
{
    Task<List<RequisitionResponseDto>> GetAsync(string? status, int? typeId, int? employeeId, DateTime? fromDate, DateTime? toDate);
    Task<RequisitionResponseDto> GetByIdAsync(int id);
    Task<RequisitionResponseDto> CreateAsync(RequisitionCreateDto dto);
    Task<RequisitionResponseDto> ApproveAsync(int id);
    Task<RequisitionResponseDto> RejectAsync(int id, ReasonDto dto);
    Task<RequisitionResponseDto> CancelAsync(int id);
}
