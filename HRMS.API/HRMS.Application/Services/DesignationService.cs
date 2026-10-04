using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Designations;
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
    public class DesignationService : IDesignationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DesignationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<DesignationResponseDto>> GetAllAsync()
        {
            var designations = await _unitOfWork.Repository<Designation>().GetAllAsync();
            return _mapper.Map<List<DesignationResponseDto>>(designations);
        }

        public async Task<DesignationResponseDto> GetByIdAsync(int id)
        {
            var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Designation), id);

            return _mapper.Map<DesignationResponseDto>(designation);
        }

        public async Task<DesignationResponseDto> CreateAsync(DesignationCreateDto dto)
        {
            var all = await _unitOfWork.Repository<Designation>().GetAllAsync();

            if (all.Any(d => d.DesignationName == dto.DesignationName))
                throw new BadRequestException($"Designation '{dto.DesignationName}' already exists.");

            var designation = _mapper.Map<Designation>(dto);

            await _unitOfWork.Repository<Designation>().AddAsync(designation);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DesignationResponseDto>(designation);
        }

        public async Task<DesignationResponseDto> UpdateAsync(int id, DesignationUpdateDto dto)
        {
            var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Designation), id);

            var all = await _unitOfWork.Repository<Designation>().GetAllAsync();

            if (all.Any(d => d.DesignationName == dto.DesignationName && d.Id != id))
                throw new BadRequestException($"Designation '{dto.DesignationName}' already exists.");

            _mapper.Map(dto, designation);
            designation.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Designation>().Update(designation);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DesignationResponseDto>(designation);
        }

        public async Task DeleteAsync(int id)
        {
            var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Designation), id);

            designation.IsActive = false;
            designation.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Designation>().Update(designation);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
