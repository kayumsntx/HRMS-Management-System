using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Employees;

namespace HRMS.Application.Interfaces.Services;

public interface IEmployeeSearchService
{
    Task<PagedResult<EmployeeListDto>> SearchAsync(EmployeeSearchDto filter);
}
