using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.EmployeeStatuses;
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
    public class EmployeeStatusService : IEmployeeStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeStatusService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmployeeStatusResponseDto>> GetAllAsync()
        {
            var statuses = await _unitOfWork.Repository<EmployeeStatus>().GetAllAsync();
            return _mapper.Map<List<EmployeeStatusResponseDto>>(statuses);
        }

        public async Task<EmployeeStatusResponseDto> GetByIdAsync(int id)
        {
            var status = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeStatus), id);

            return _mapper.Map<EmployeeStatusResponseDto>(status);
        }

        public async Task<EmployeeStatusResponseDto> CreateAsync(EmployeeStatusCreateDto dto)
        {
            var all = await _unitOfWork.Repository<EmployeeStatus>().GetAllAsync();

            if (all.Any(s => s.StatusName == dto.StatusName))
                throw new BadRequestException($"Employee Status '{dto.StatusName}' already exists.");

            var status = _mapper.Map<EmployeeStatus>(dto);

            await _unitOfWork.Repository<EmployeeStatus>().AddAsync(status);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EmployeeStatusResponseDto>(status);
        }

        public async Task<EmployeeStatusResponseDto> UpdateAsync(int id, EmployeeStatusUpdateDto dto)
        {
            var status = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeStatus), id);

            var all = await _unitOfWork.Repository<EmployeeStatus>().GetAllAsync();

            if (all.Any(s => s.StatusName == dto.StatusName && s.Id != id))
                throw new BadRequestException($"Employee Status '{dto.StatusName}' already exists.");

            _mapper.Map(dto, status);
            status.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<EmployeeStatus>().Update(status);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EmployeeStatusResponseDto>(status);
        }

        public async Task DeleteAsync(int id)
        {
            var status = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeStatus), id);

            status.IsActive = false;
            status.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<EmployeeStatus>().Update(status);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
