using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.EmployeeEducations;
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
    public class EmployeeEducationService : IEmployeeEducationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeEducationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmployeeEducationResponseDto>> GetByEmployeeIdAsync(int employeeId)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            var all = await _unitOfWork.Repository<EmployeeEducation>().GetAllAsync();
            var filtered = all.Where(x => x.EmployeeId == employeeId).ToList();

            var result = _mapper.Map<List<EmployeeEducationResponseDto>>(filtered);
            result.ForEach(x => x.EmployeeName = employee.FullName);

            return result;
        }

        public async Task<EmployeeEducationResponseDto> GetByIdAsync(int id)
        {
            var education = await _unitOfWork.Repository<EmployeeEducation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeEducation), id);

            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(education.EmployeeId);

            var dto = _mapper.Map<EmployeeEducationResponseDto>(education);
            dto.EmployeeName = employee?.FullName;

            return dto;
        }

        public async Task<EmployeeEducationResponseDto> CreateAsync(EmployeeEducationCreateDto dto)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

            var education = _mapper.Map<EmployeeEducation>(dto);

            await _unitOfWork.Repository<EmployeeEducation>().AddAsync(education);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<EmployeeEducationResponseDto>(education);
            result.EmployeeName = employee.FullName;
            return result;
        }

        public async Task<EmployeeEducationResponseDto> UpdateAsync(int id, EmployeeEducationCreateDto dto)
        {
            var education = await _unitOfWork.Repository<EmployeeEducation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeEducation), id);

            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

            _mapper.Map(dto, education);
            education.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<EmployeeEducation>().Update(education);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<EmployeeEducationResponseDto>(education);
            result.EmployeeName = employee.FullName;
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var education = await _unitOfWork.Repository<EmployeeEducation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeEducation), id);

            _unitOfWork.Repository<EmployeeEducation>().Delete(education);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
