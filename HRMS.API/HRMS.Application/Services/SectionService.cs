using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Sections;
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
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SectionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SectionResponseDto>> GetAllAsync()
        {
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
            var departments = await _unitOfWork.Repository<Department>().GetAllAsync();

            var result = _mapper.Map<List<SectionResponseDto>>(sections);

            foreach (var dto in result)
            {
                dto.DepartmentName = departments.FirstOrDefault(d => d.Id == dto.DepartmentId)?.DepartmentName;
            }

            return result;
        }

        public async Task<SectionResponseDto> GetByIdAsync(int id)
        {
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Section), id);

            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(section.DepartmentId);

            var dto = _mapper.Map<SectionResponseDto>(section);
            dto.DepartmentName = department?.DepartmentName;

            return dto;
        }

        public async Task<SectionResponseDto> CreateAsync(SectionCreateDto dto)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(dto.DepartmentId)
                ?? throw new NotFoundException(nameof(Department), dto.DepartmentId);

            var existingSections = await _unitOfWork.Repository<Section>().GetAllAsync();
            if (existingSections.Any(s => s.DepartmentId == dto.DepartmentId && s.SectionName == dto.SectionName))
                throw new BadRequestException($"Section '{dto.SectionName}' already exists in this Department.");

            var section = _mapper.Map<Section>(dto);

            await _unitOfWork.Repository<Section>().AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<SectionResponseDto>(section);
            result.DepartmentName = department.DepartmentName;
            return result;
        }

        public async Task<SectionResponseDto> UpdateAsync(int id, SectionUpdateDto dto)
        {
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Section), id);

            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(dto.DepartmentId)
                ?? throw new NotFoundException(nameof(Department), dto.DepartmentId);

            var existingSections = await _unitOfWork.Repository<Section>().GetAllAsync();
            if (existingSections.Any(s => s.DepartmentId == dto.DepartmentId && s.SectionName == dto.SectionName && s.Id != id))
                throw new BadRequestException($"Section '{dto.SectionName}' already exists in this Department.");

            _mapper.Map(dto, section);
            section.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Section>().Update(section);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<SectionResponseDto>(section);
            result.DepartmentName = department.DepartmentName;
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var section = await _unitOfWork.Repository<Section>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Section), id);

            section.IsActive = false;
            section.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Section>().Update(section);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
