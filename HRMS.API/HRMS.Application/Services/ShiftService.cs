using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Shifts;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShiftService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ShiftResponseDto>> GetAllAsync()
    {
        var shifts = await _unitOfWork.Repository<Shift>().GetAllAsync();
        return _mapper.Map<List<ShiftResponseDto>>(shifts);
    }

    public async Task<ShiftResponseDto> GetByIdAsync(int id)
    {
        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shift), id);

        return _mapper.Map<ShiftResponseDto>(shift);
    }

    public async Task<ShiftResponseDto> CreateAsync(ShiftCreateDto dto)
    {
        var all = await _unitOfWork.Repository<Shift>().GetAllAsync();

        if (all.Any(s => string.Equals(s.ShiftName, dto.ShiftName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Shift '{dto.ShiftName}' already exists.");

        var shift = _mapper.Map<Shift>(dto);

        await _unitOfWork.Repository<Shift>().AddAsync(shift);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShiftResponseDto>(shift);
    }

    public async Task<ShiftResponseDto> UpdateAsync(int id, ShiftUpdateDto dto)
    {
        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shift), id);

        var all = await _unitOfWork.Repository<Shift>().GetAllAsync();

        if (all.Any(s => s.Id != id && string.Equals(s.ShiftName, dto.ShiftName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"Shift '{dto.ShiftName}' already exists.");

        _mapper.Map(dto, shift);
        shift.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Shift>().Update(shift);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShiftResponseDto>(shift);
    }

    public async Task DeleteAsync(int id)
    {
        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shift), id);

        shift.IsActive = false;
        shift.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Shift>().Update(shift);
        await _unitOfWork.SaveChangesAsync();
    }
}
