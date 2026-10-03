using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Units;
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
    public class UnitService : IUnitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UnitService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UnitResponseDto>> GetAllAsync()
        {
            var units = await _unitOfWork.Repository<Unit>().GetAllAsync();
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();

            var result = _mapper.Map<List<UnitResponseDto>>(units);

            // CompanyName manual (without Include )
            foreach (var dto in result)
            {
                dto.CompanyName = companies.FirstOrDefault(c => c.Id == dto.CompanyId)?.CompanyName;
            }

            return result;
        }

        public async Task<UnitResponseDto> GetByIdAsync(int id)
        {
            var unit = await _unitOfWork.Repository<Unit>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Unit), id);

            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(unit.CompanyId);

            var dto = _mapper.Map<UnitResponseDto>(unit);
            dto.CompanyName = company?.CompanyName;

            return dto;
        }

        public async Task<UnitResponseDto> CreateAsync(UnitCreateDto dto)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(dto.CompanyId)
                ?? throw new NotFoundException(nameof(Company), dto.CompanyId);

            var existingUnits = await _unitOfWork.Repository<Unit>().GetAllAsync();
            if (existingUnits.Any(u => u.CompanyId == dto.CompanyId && u.UnitName == dto.UnitName))
                throw new BadRequestException($"In Company '{dto.UnitName}' this Unit name already exist।");

            var unit = _mapper.Map<Unit>(dto);

            await _unitOfWork.Repository<Unit>().AddAsync(unit);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UnitResponseDto>(unit);
            result.CompanyName = company.CompanyName;
            return result;
        }

        public async Task<UnitResponseDto> UpdateAsync(int id, UnitUpdateDto dto)
        {
            var unit = await _unitOfWork.Repository<Unit>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Unit), id);

            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(dto.CompanyId)
                ?? throw new NotFoundException(nameof(Company), dto.CompanyId);

            var existingUnits = await _unitOfWork.Repository<Unit>().GetAllAsync();
            if (existingUnits.Any(u => u.CompanyId == dto.CompanyId && u.UnitName == dto.UnitName && u.Id != id))
                throw new BadRequestException($"In Company '{dto.UnitName}'this Unit name already exist।");

            _mapper.Map(dto, unit);
            unit.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Unit>().Update(unit);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<UnitResponseDto>(unit);
            result.CompanyName = company.CompanyName;
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var unit = await _unitOfWork.Repository<Unit>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Unit), id);

            unit.IsActive = false;
            unit.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Unit>().Update(unit);
            await _unitOfWork.SaveChangesAsync();
        } }
    }
