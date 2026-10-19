using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeaveBalanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<LeaveBalanceResponseDto>> GetByEmployeeIdAsync(int employeeId, int? year)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var balances = (await _unitOfWork.Repository<EmployeeLeaveBalance>().GetAllAsync())
            .Where(b => b.EmployeeId == employeeId && (!year.HasValue || b.LeaveYear == year.Value))
            .OrderByDescending(b => b.LeaveYear)
            .ToList();

        var leaveTypes = await _unitOfWork.Repository<LeaveType>().GetAllAsync();

        var result = _mapper.Map<List<LeaveBalanceResponseDto>>(balances);
        foreach (var dto in result)
        {
            dto.EmployeeName = employee.FullName;
            dto.LeaveTypeName = leaveTypes.FirstOrDefault(l => l.Id == dto.LeaveTypeId)?.LeaveTypeName;
        }

        return result;
    }

    public async Task<LeaveBalanceResponseDto> GetByIdAsync(int id)
    {
        var balance = await _unitOfWork.Repository<EmployeeLeaveBalance>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeaveBalance), id);

        return await MapToResponseAsync(balance);
    }

    public async Task<LeaveBalanceResponseDto> CreateAsync(LeaveBalanceCreateDto dto)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId) is null)
            throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (await _unitOfWork.Repository<LeaveType>().GetByIdAsync(dto.LeaveTypeId) is null)
            throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId);

        var existing = await _unitOfWork.Repository<EmployeeLeaveBalance>().GetAllAsync();
        if (existing.Any(b => b.EmployeeId == dto.EmployeeId && b.LeaveTypeId == dto.LeaveTypeId && b.LeaveYear == dto.LeaveYear))
            throw new BadRequestException("A leave balance for this Employee, Leave Type and Year already exists.");

        var balance = _mapper.Map<EmployeeLeaveBalance>(dto);

        await _unitOfWork.Repository<EmployeeLeaveBalance>().AddAsync(balance);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseAsync(balance);
    }

    public async Task<LeaveBalanceResponseDto> UpdateAsync(int id, LeaveBalanceUpdateDto dto)
    {
        var balance = await _unitOfWork.Repository<EmployeeLeaveBalance>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeaveBalance), id);

        if (dto.OpeningBalance < balance.UsedDays)
            throw new BadRequestException($"Opening Balance cannot be less than the already used days ({balance.UsedDays}).");

        balance.OpeningBalance = dto.OpeningBalance;
        balance.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeLeaveBalance>().Update(balance);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseAsync(balance);
    }

    public async Task DeleteAsync(int id)
    {
        var balance = await _unitOfWork.Repository<EmployeeLeaveBalance>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeaveBalance), id);

        if (balance.UsedDays > 0)
            throw new BadRequestException("A leave balance that has already been used cannot be deleted.");

        _unitOfWork.Repository<EmployeeLeaveBalance>().Delete(balance);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LeaveBalanceResponseDto> MapToResponseAsync(EmployeeLeaveBalance balance)
    {
        var dto = _mapper.Map<LeaveBalanceResponseDto>(balance);

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(balance.EmployeeId);
        var leaveType = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(balance.LeaveTypeId);

        dto.EmployeeName = employee?.FullName;
        dto.LeaveTypeName = leaveType?.LeaveTypeName;

        return dto;
    }
}
