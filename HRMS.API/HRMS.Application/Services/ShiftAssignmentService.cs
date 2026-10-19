using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.ShiftAssignments;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class ShiftAssignmentService : IShiftAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShiftAssignmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ShiftAssignmentResponseDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var assignments = (await _unitOfWork.Repository<ShiftAssignment>().GetAllAsync())
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.EffectiveFrom)
            .ToList();

        var shifts = await _unitOfWork.Repository<Shift>().GetAllAsync();

        var result = _mapper.Map<List<ShiftAssignmentResponseDto>>(assignments);
        foreach (var dto in result)
        {
            dto.EmployeeCode = employee.EmployeeCode;
            dto.EmployeeName = employee.FullName;
            dto.ShiftName = shifts.FirstOrDefault(s => s.Id == dto.ShiftId)?.ShiftName;
        }

        return result;
    }

    public async Task<ShiftAssignmentResponseDto> GetByIdAsync(int id)
    {
        var assignment = await _unitOfWork.Repository<ShiftAssignment>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ShiftAssignment), id);

        return await MapToResponseAsync(assignment);
    }

    public async Task<ShiftAssignmentResponseDto> CreateAsync(ShiftAssignmentCreateDto dto)
    {
        await ValidateAsync(dto, null);

        var assignment = _mapper.Map<ShiftAssignment>(dto);

        await _unitOfWork.Repository<ShiftAssignment>().AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseAsync(assignment);
    }

    public async Task<ShiftAssignmentResponseDto> UpdateAsync(int id, ShiftAssignmentCreateDto dto)
    {
        var assignment = await _unitOfWork.Repository<ShiftAssignment>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ShiftAssignment), id);

        await ValidateAsync(dto, id);

        _mapper.Map(dto, assignment);
        assignment.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<ShiftAssignment>().Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseAsync(assignment);
    }

    public async Task DeleteAsync(int id)
    {
        var assignment = await _unitOfWork.Repository<ShiftAssignment>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ShiftAssignment), id);

        _unitOfWork.Repository<ShiftAssignment>().Delete(assignment);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------------- Private Helpers ----------------

    private async Task ValidateAsync(ShiftAssignmentCreateDto dto, int? currentId)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId) is null)
            throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(dto.ShiftId)
            ?? throw new NotFoundException(nameof(Shift), dto.ShiftId);

        if (!shift.IsActive)
            throw new BadRequestException("This Shift is inactive.");

        // An employee cannot have two overlapping shift assignments.
        var newFrom = dto.EffectiveFrom.Date;
        var newTo = dto.EffectiveTo?.Date ?? DateTime.MaxValue.Date;

        var existing = await _unitOfWork.Repository<ShiftAssignment>().GetAllAsync();
        var overlaps = existing.Any(a =>
            a.EmployeeId == dto.EmployeeId &&
            a.Id != currentId &&
            newFrom <= (a.EffectiveTo?.Date ?? DateTime.MaxValue.Date) &&
            newTo >= a.EffectiveFrom.Date);

        if (overlaps)
            throw new BadRequestException("This Employee already has a Shift Assignment that overlaps with the given date range.");
    }

    private async Task<ShiftAssignmentResponseDto> MapToResponseAsync(ShiftAssignment assignment)
    {
        var dto = _mapper.Map<ShiftAssignmentResponseDto>(assignment);

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(assignment.EmployeeId);
        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(assignment.ShiftId);

        dto.EmployeeCode = employee?.EmployeeCode;
        dto.EmployeeName = employee?.FullName;
        dto.ShiftName = shift?.ShiftName;

        return dto;
    }
}
