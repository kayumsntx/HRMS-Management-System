using HRMS.Application.DTOs.EmployeeDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeDocumentService
    {
        Task<List<EmployeeDocumentResponseDto>> GetByEmployeeIdAsync(int employeeId);
        Task<EmployeeDocumentResponseDto> GetByIdAsync(int id);
        Task<EmployeeDocumentResponseDto> CreateAsync(EmployeeDocumentCreateDto dto);
        Task DeleteAsync(int id);
    }
}
