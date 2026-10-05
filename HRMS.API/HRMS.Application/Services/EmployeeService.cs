using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmployeeListDto>> GetAllAsync()
        {
            var employees = await _unitOfWork.Repository<Employee>().GetAllAsync();

            var designations = await _unitOfWork.Repository<Designation>().GetAllAsync();
            var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();

            var result = _mapper.Map<List<EmployeeListDto>>(employees);

            foreach (var dto in result)
            {
                var employee = employees.First(e => e.Id == dto.Id);

                dto.DesignationName = designations.FirstOrDefault(d => d.Id == employee.DesignationId)?.DesignationName;
                dto.DepartmentName = departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.DepartmentName;
                dto.CompanyName = companies.FirstOrDefault(c => c.Id == employee.CompanyId)?.CompanyName;
            }

            return result;
        }

        public async Task<EmployeeResponseDto> GetByIdAsync(int id)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Employee), id);

            return await MapToResponseDtoAsync(employee);
        }

        public async Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto)
        {
            await ValidateForeignKeysAsync(dto.EmployeeStatusId, dto.DesignationId, dto.GradeId,
                dto.CompanyId, dto.UnitId, dto.DepartmentId, dto.SectionId, dto.ReportingManagerId);

            if (!string.IsNullOrEmpty(dto.NationalIdNo))
            {
                var allEmployees = await _unitOfWork.Repository<Employee>().GetAllAsync();
                if (allEmployees.Any(e => e.NationalIdNo == dto.NationalIdNo))
                    throw new BadRequestException($"National ID '{dto.NationalIdNo}' already exists.");
            }

            var employee = _mapper.Map<Employee>(dto);

            // ---- EmployeeCode Auto-Generate  string 
            employee.EmployeeCode = await GenerateEmployeeCodeAsync();

            await _unitOfWork.Repository<Employee>().AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDtoAsync(employee);
        }

        public async Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeUpdateDto dto)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Employee), id);

            await ValidateForeignKeysAsync(dto.EmployeeStatusId, dto.DesignationId, dto.GradeId,
                dto.CompanyId, dto.UnitId, dto.DepartmentId, dto.SectionId, dto.ReportingManagerId);

            //var allEmployees = await _unitOfWork.Repository<Employee>().GetAllAsync();

            //if (allEmployees.Any(e => e.EmployeeCode == dto.EmployeeCode && e.Id != id))
            //    throw new BadRequestException($"Employee Code '{dto.EmployeeCode}' already exists.");

            if (dto.ReportingManagerId == id)
                throw new BadRequestException("An employee cannot be their own Reporting Manager.");

            _mapper.Map(dto, employee);
            employee.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Employee>().Update(employee);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDtoAsync(employee);
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Employee), id);

            employee.IsActive = false;
            employee.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Employee>().Update(employee);
            await _unitOfWork.SaveChangesAsync();
        }

        // ---------------- Private Helper Methods ----------------



        private async Task ValidateForeignKeysAsync(
            int employeeStatusId, int designationId, int? gradeId,
            int companyId, int? unitId, int departmentId, int? sectionId, int? reportingManagerId)
        {
            if (await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(employeeStatusId) is null)
                throw new NotFoundException(nameof(EmployeeStatus), employeeStatusId);

            if (await _unitOfWork.Repository<Designation>().GetByIdAsync(designationId) is null)
                throw new NotFoundException(nameof(Designation), designationId);

            if (gradeId.HasValue && await _unitOfWork.Repository<Grade>().GetByIdAsync(gradeId.Value) is null)
                throw new NotFoundException(nameof(Grade), gradeId.Value);

            if (await _unitOfWork.Repository<Company>().GetByIdAsync(companyId) is null)
                throw new NotFoundException(nameof(Company), companyId);

            if (unitId.HasValue && await _unitOfWork.Repository<Unit>().GetByIdAsync(unitId.Value) is null)
                throw new NotFoundException(nameof(Unit), unitId.Value);

            if (await _unitOfWork.Repository<Department>().GetByIdAsync(departmentId) is null)
                throw new NotFoundException(nameof(Department), departmentId);

            if (sectionId.HasValue && await _unitOfWork.Repository<Section>().GetByIdAsync(sectionId.Value) is null)
                throw new NotFoundException(nameof(Section), sectionId.Value);

            if (reportingManagerId.HasValue && await _unitOfWork.Repository<Employee>().GetByIdAsync(reportingManagerId.Value) is null)
                throw new NotFoundException(nameof(Employee), reportingManagerId.Value);
        }

        private async Task<EmployeeResponseDto> MapToResponseDtoAsync(Employee employee)
        {
            var dto = _mapper.Map<EmployeeResponseDto>(employee);

            dto.EmployeeStatusName = (await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(employee.EmployeeStatusId))?.StatusName;
            dto.DesignationName = (await _unitOfWork.Repository<Designation>().GetByIdAsync(employee.DesignationId))?.DesignationName;
            dto.CompanyName = (await _unitOfWork.Repository<Company>().GetByIdAsync(employee.CompanyId))?.CompanyName;
            dto.DepartmentName = (await _unitOfWork.Repository<Department>().GetByIdAsync(employee.DepartmentId))?.DepartmentName;

            if (employee.GradeId.HasValue)
                dto.GradeName = (await _unitOfWork.Repository<Grade>().GetByIdAsync(employee.GradeId.Value))?.GradeName;

            if (employee.UnitId.HasValue)
                dto.UnitName = (await _unitOfWork.Repository<Unit>().GetByIdAsync(employee.UnitId.Value))?.UnitName;

            if (employee.SectionId.HasValue)
                dto.SectionName = (await _unitOfWork.Repository<Section>().GetByIdAsync(employee.SectionId.Value))?.SectionName;

            if (employee.ReportingManagerId.HasValue)
                dto.ReportingManagerName = (await _unitOfWork.Repository<Employee>().GetByIdAsync(employee.ReportingManagerId.Value))?.FullName;

            return dto;
        }


        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var allEmployees = await _unitOfWork.Repository<Employee>().GetAllAsync();

            var maxNumber = allEmployees
                .Select(e => e.EmployeeCode)
                .Where(code => int.TryParse(code, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(1000)
                .Max();

            var nextNumber = maxNumber + 1;

            return nextNumber.ToString();   // "1001", "1002", "1003"...
        }
    }
}
