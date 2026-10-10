using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Roles;
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
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<RoleResponseDto>> GetAllAsync()
        {
            var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
            return _mapper.Map<List<RoleResponseDto>>(roles);
        }

        public async Task<RoleResponseDto> GetByIdAsync(int id)
        {
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Role), id);

            return _mapper.Map<RoleResponseDto>(role);
        }

        public async Task<RoleResponseDto> CreateAsync(RoleCreateDto dto)
        {
            var all = await _unitOfWork.Repository<Role>().GetAllAsync();

            if (all.Any(r => r.RoleName == dto.RoleName))
                throw new BadRequestException($"Role '{dto.RoleName}' already exists.");

            var role = _mapper.Map<Role>(dto);

            await _unitOfWork.Repository<Role>().AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RoleResponseDto>(role);
        }

        public async Task<RoleResponseDto> UpdateAsync(int id, RoleUpdateDto dto)
        {
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Role), id);

            var all = await _unitOfWork.Repository<Role>().GetAllAsync();

            if (all.Any(r => r.RoleName == dto.RoleName && r.Id != id))
                throw new BadRequestException($"Role '{dto.RoleName}' already exists.");

            _mapper.Map(dto, role);
            role.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Role>().Update(role);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RoleResponseDto>(role);
        }

        public async Task DeleteAsync(int id)
        {
            var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Role), id);

            role.IsActive = false;
            role.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Role>().Update(role);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
