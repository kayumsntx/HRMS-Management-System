using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Grades;
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
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<GradeResponseDto>> GetAllAsync()
        {
            var grades = await _unitOfWork.Repository<Grade>().GetAllAsync();
            return _mapper.Map<List<GradeResponseDto>>(grades);
        }

        public async Task<GradeResponseDto> GetByIdAsync(int id)
        {
            var grade = await _unitOfWork.Repository<Grade>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Grade), id);

            return _mapper.Map<GradeResponseDto>(grade);
        }

        public async Task<GradeResponseDto> CreateAsync(GradeCreateDto dto)
        {
            var all = await _unitOfWork.Repository<Grade>().GetAllAsync();

            if (all.Any(g => g.GradeName == dto.GradeName))
                throw new BadRequestException($"Grade '{dto.GradeName}' already exists.");

            var grade = _mapper.Map<Grade>(dto);

            await _unitOfWork.Repository<Grade>().AddAsync(grade);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GradeResponseDto>(grade);
        }

        public async Task<GradeResponseDto> UpdateAsync(int id, GradeUpdateDto dto)
        {
            var grade = await _unitOfWork.Repository<Grade>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Grade), id);

            var all = await _unitOfWork.Repository<Grade>().GetAllAsync();

            if (all.Any(g => g.GradeName == dto.GradeName && g.Id != id))
                throw new BadRequestException($"Grade '{dto.GradeName}' already exists.");

            _mapper.Map(dto, grade);
            grade.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Grade>().Update(grade);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GradeResponseDto>(grade);
        }

        public async Task DeleteAsync(int id)
        {
            var grade = await _unitOfWork.Repository<Grade>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Grade), id);

            grade.IsActive = false;
            grade.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Grade>().Update(grade);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
