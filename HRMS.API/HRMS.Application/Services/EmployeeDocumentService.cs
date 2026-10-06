using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.EmployeeDocuments;
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
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeDocumentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<EmployeeDocumentResponseDto>> GetByEmployeeIdAsync(int employeeId)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
                ?? throw new NotFoundException(nameof(Employee), employeeId);

            var all = await _unitOfWork.Repository<EmployeeDocument>().GetAllAsync();
            var filtered = all.Where(x => x.EmployeeId == employeeId).ToList();

            var result = _mapper.Map<List<EmployeeDocumentResponseDto>>(filtered);
            result.ForEach(x => x.EmployeeName = employee.FullName);

            return result;
        }

        public async Task<EmployeeDocumentResponseDto> GetByIdAsync(int id)
        {
            var document = await _unitOfWork.Repository<EmployeeDocument>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeDocument), id);

            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(document.EmployeeId);

            var dto = _mapper.Map<EmployeeDocumentResponseDto>(document);
            dto.EmployeeName = employee?.FullName;

            return dto;
        }

        public async Task<EmployeeDocumentResponseDto> CreateAsync(EmployeeDocumentCreateDto dto)
        {
            var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

            var document = _mapper.Map<EmployeeDocument>(dto);
            document.UploadedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<EmployeeDocument>().AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<EmployeeDocumentResponseDto>(document);
            result.EmployeeName = employee.FullName;
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var document = await _unitOfWork.Repository<EmployeeDocument>().GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(EmployeeDocument), id);

            _unitOfWork.Repository<EmployeeDocument>().Delete(document);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
