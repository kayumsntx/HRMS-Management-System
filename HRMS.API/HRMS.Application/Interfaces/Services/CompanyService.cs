using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Companies;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompanyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<CompanyResponseDto>> GetAllAsync()
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            return _mapper.Map<List<CompanyResponseDto>>(companies);
        }

        public async Task<CompanyResponseDto> GetByIdAsync(int id)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Company), id);

            return _mapper.Map<CompanyResponseDto>(company);
        }

        public async Task<CompanyResponseDto> CreateAsync(CompanyCreateDto dto)
        {
            //Business Rule: Duplicate Company Code check
            var allCompanies = await _unitOfWork.Repository<Company>().GetAllAsync();

            if (allCompanies.Any(c => c.CompanyCode == dto.CompanyCode))
                throw new BadRequestException($"Company Code '{dto.CompanyCode}' Already Used");

            if (allCompanies.Any(c => c.CompanyName == dto.CompanyName))
                throw new BadRequestException($"Company Name '{dto.CompanyName}' Already Used");

            var company = _mapper.Map<Company>(dto);

            await _unitOfWork.Repository<Company>().AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CompanyResponseDto>(company);
        }

        public async Task<CompanyResponseDto> UpdateAsync(int id, CompanyUpdateDto dto)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Company), id);

            // Business Rule:  Company  Code/Name clash with other company
            var allCompanies = await _unitOfWork.Repository<Company>().GetAllAsync();

            if (allCompanies.Any(c => c.CompanyCode == dto.CompanyCode && c.Id != id))
                throw new BadRequestException($"Company Code '{dto.CompanyCode}' অন্য একটি Company তে ব্যবহৃত হয়েছে।");

            // mapping — DTO 's existing entity 
            _mapper.Map(dto, company);
            company.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CompanyResponseDto>(company);
        }

        public async Task DeleteAsync(int id)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Company), id);

            // Soft Delete, make  IsActive = false 
            company.IsActive = false;
            company.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
