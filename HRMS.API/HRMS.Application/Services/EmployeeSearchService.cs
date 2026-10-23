using AutoMapper;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class EmployeeSearchService : IEmployeeSearchService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public EmployeeSearchService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmployeeListDto>> SearchAsync(EmployeeSearchDto filter)
    {
        var term = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
        var departmentId = filter.DepartmentId;
        var designationId = filter.DesignationId;
        var statusId = filter.EmployeeStatusId;
        var isActive = filter.IsActive;

        var (items, total) = await _uow.Repository<Employee>().FindPagedAsync(e =>
                (term == null || e.FullName.Contains(term) || e.EmployeeCode.Contains(term) || (e.MobileNumber != null && e.MobileNumber.Contains(term))) &&
                (!departmentId.HasValue || e.DepartmentId == departmentId.Value) &&
                (!designationId.HasValue || e.DesignationId == designationId.Value) &&
                (!statusId.HasValue || e.EmployeeStatusId == statusId.Value) &&
                (!isActive.HasValue || e.IsActive == isActive.Value),
            filter.PageNumber, filter.PageSize);

        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);
        var companies = (await _uow.Repository<Company>().GetAllAsync()).ToDictionary(c => c.Id, c => c.CompanyName);

        var rows = _mapper.Map<List<EmployeeListDto>>(items);
        foreach (var dto in rows)
        {
            var employee = items.First(e => e.Id == dto.Id);
            dto.DesignationName = designations.GetValueOrDefault(employee.DesignationId);
            dto.DepartmentName = departments.GetValueOrDefault(employee.DepartmentId);
            dto.CompanyName = companies.GetValueOrDefault(employee.CompanyId);
        }

        return new PagedResult<EmployeeListDto>
        {
            Items = rows,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }
}
