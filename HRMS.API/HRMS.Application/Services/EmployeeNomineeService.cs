using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.EmployeeNominees;
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
    public class EmployeeNomineeService : IEmployeeNomineeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeNomineeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmployeeNomineeResponseDto>> GetByEmployeeIdAsync(int employeeId)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            var all = await _unitOfWork.Repository<EmployeeNominee>().GetAllAsync();
            var filtered = all.Where(x => x.EmployeeId == employeeId).ToList();

            var result = _mapper.Map<List<EmployeeNomineeResponseDto>>(filtered);
            result.ForEach(x => x.EmployeeName = employee.FullName);

            return result;
        }

        public async Task<EmployeeNomineeResponseDto> GetByIdAsync(int id)
        {
            var nominee = await _unitOfWork.Repository<EmployeeNominee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeNominee), id);

            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(nominee.EmployeeId);

            var dto = _mapper.Map<EmployeeNomineeResponseDto>(nominee);
            dto.EmployeeName = employee?.FullName;

            return dto;
        }

        public async Task<EmployeeNomineeResponseDto> CreateAsync(EmployeeNomineeCreateDto dto)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

            // ---- Business Rule: মোট SharePercentage যেন 100% অতিক্রম না করে ----
            var allNominees = await _unitOfWork.Repository<EmployeeNominee>().GetAllAsync();
            var existingTotal = allNominees
                .Where(n => n.EmployeeId == dto.EmployeeId)
                .Sum(n => n.SharePercentage ?? 0);

            if (dto.SharePercentage.HasValue && (existingTotal + dto.SharePercentage.Value) > 100)
                throw new BadRequestException("Total Share Percentage for all nominees cannot exceed 100%.");

            var nominee = _mapper.Map<EmployeeNominee>(dto);

            await _unitOfWork.Repository<EmployeeNominee>().AddAsync(nominee);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<EmployeeNomineeResponseDto>(nominee);
            result.EmployeeName = employee.FullName;
            return result;
        }

        public async Task<EmployeeNomineeResponseDto> UpdateAsync(int id, EmployeeNomineeCreateDto dto)
        {
            var nominee = await _unitOfWork.Repository<EmployeeNominee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeNominee), id);

            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

            var allNominees = await _unitOfWork.Repository<EmployeeNominee>().GetAllAsync();
            var existingTotal = allNominees
                .Where(n => n.EmployeeId == dto.EmployeeId && n.Id != id)
                .Sum(n => n.SharePercentage ?? 0);

            if (dto.SharePercentage.HasValue && (existingTotal + dto.SharePercentage.Value) > 100)
                throw new BadRequestException("Total Share Percentage for all nominees cannot exceed 100%.");

            _mapper.Map(dto, nominee);
            nominee.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<EmployeeNominee>().Update(nominee);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<EmployeeNomineeResponseDto>(nominee);
            result.EmployeeName = employee.FullName;
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var nominee = await _unitOfWork.Repository<EmployeeNominee>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeNominee), id);

            _unitOfWork.Repository<EmployeeNominee>().Delete(nominee);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
