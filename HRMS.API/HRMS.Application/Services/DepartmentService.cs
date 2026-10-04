using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Departments;
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
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<DepartmentResponseDto>> GetAllAsync()
        {
            var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
            return _mapper.Map<List<DepartmentResponseDto>>(departments);
        }

        public async Task<DepartmentResponseDto> GetByIdAsync(int id)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Department), id);

            return _mapper.Map<DepartmentResponseDto>(department);
        }

        public async Task<DepartmentResponseDto> CreateAsync(DepartmentCreateDto dto)
        {
            var allDepartments = await _unitOfWork.Repository<Department>().GetAllAsync();

            if (allDepartments.Any(d => d.DepartmentName == dto.DepartmentName))
                throw new BadRequestException($"Department '{dto.DepartmentName}' already exists.");

            var department = _mapper.Map<Department>(dto);

            await _unitOfWork.Repository<Department>().AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepartmentResponseDto>(department);
        }

        public async Task<DepartmentResponseDto> UpdateAsync(int id, DepartmentUpdateDto dto)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Department), id);

            var allDepartments = await _unitOfWork.Repository<Department>().GetAllAsync();

            if (allDepartments.Any(d => d.DepartmentName == dto.DepartmentName && d.Id != id))
                throw new BadRequestException($"Department '{dto.DepartmentName}' already exists.");

            _mapper.Map(dto, department);
            department.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Department>().Update(department);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DepartmentResponseDto>(department);
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Department), id);

            department.IsActive = false;
            department.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Department>().Update(department);
            await _unitOfWork.SaveChangesAsync();
        }
    }
    }
