using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeaveTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<LeaveTypeResponseDto>> GetAllAsync()
    {
        var items = await _unitOfWork.Repository<LeaveType>().GetAllAsync();
        return _mapper.Map<List<LeaveTypeResponseDto>>(items);
    }

    public async Task<LeaveTypeResponseDto> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(LeaveType), id);

        return _mapper.Map<LeaveTypeResponseDto>(item);
    }

    public async Task<LeaveTypeResponseDto> CreateAsync(LeaveTypeCreateDto dto)
    {
        var all = await _unitOfWork.Repository<LeaveType>().GetAllAsync();

        if (all.Any(l => string.Equals(l.LeaveTypeName, dto.LeaveTypeName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Leave Type '{dto.LeaveTypeName}' already exists.");

        var item = _mapper.Map<LeaveType>(dto);

        await _unitOfWork.Repository<LeaveType>().AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveTypeResponseDto>(item);
    }

    public async Task<LeaveTypeResponseDto> UpdateAsync(int id, LeaveTypeUpdateDto dto)
    {
        var item = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(LeaveType), id);

        var all = await _unitOfWork.Repository<LeaveType>().GetAllAsync();

        if (all.Any(l => l.Id != id && string.Equals(l.LeaveTypeName, dto.LeaveTypeName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Leave Type '{dto.LeaveTypeName}' already exists.");

        _mapper.Map(dto, item);
        item.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<LeaveType>().Update(item);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LeaveTypeResponseDto>(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(LeaveType), id);

        item.IsActive = false;
        item.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<LeaveType>().Update(item);
        await _unitOfWork.SaveChangesAsync();
    }
}
