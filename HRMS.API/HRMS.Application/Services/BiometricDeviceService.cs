using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class BiometricDeviceService : IBiometricDeviceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BiometricDeviceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<BiometricDeviceResponseDto>> GetAllAsync()
    {
        var devices = await _unitOfWork.Repository<BiometricDevice>().GetAllAsync();
        return _mapper.Map<List<BiometricDeviceResponseDto>>(devices);
    }

    public async Task<BiometricDeviceResponseDto> GetByIdAsync(int id)
    {
        var device = await _unitOfWork.Repository<BiometricDevice>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(BiometricDevice), id);

        return _mapper.Map<BiometricDeviceResponseDto>(device);
    }

    public async Task<BiometricDeviceResponseDto> CreateAsync(BiometricDeviceCreateDto dto)
    {
        var all = await _unitOfWork.Repository<BiometricDevice>().GetAllAsync();

        if (all.Any(d => string.Equals(d.DeviceSerialNumber, dto.DeviceSerialNumber, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"A device with serial number '{dto.DeviceSerialNumber}' already exists.");

        var device = _mapper.Map<BiometricDevice>(dto);

        await _unitOfWork.Repository<BiometricDevice>().AddAsync(device);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BiometricDeviceResponseDto>(device);
    }

    public async Task<BiometricDeviceResponseDto> UpdateAsync(int id, BiometricDeviceUpdateDto dto)
    {
        var device = await _unitOfWork.Repository<BiometricDevice>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(BiometricDevice), id);

        var all = await _unitOfWork.Repository<BiometricDevice>().GetAllAsync();

        if (all.Any(d => d.Id != id && string.Equals(d.DeviceSerialNumber, dto.DeviceSerialNumber, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"A device with serial number '{dto.DeviceSerialNumber}' already exists.");

        _mapper.Map(dto, device);
        device.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<BiometricDevice>().Update(device);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BiometricDeviceResponseDto>(device);
    }

    public async Task DeleteAsync(int id)
    {
        var device = await _unitOfWork.Repository<BiometricDevice>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(BiometricDevice), id);

        device.IsActive = false;
        device.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<BiometricDevice>().Update(device);
        await _unitOfWork.SaveChangesAsync();
    }
}
