using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Holidays;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HolidayService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<HolidayResponseDto>> GetAllAsync(int? year)
    {
        var targetYear = year ?? DateTime.Now.Year;
        if (targetYear < 2000 || targetYear > 2100)
            throw new BadRequestException("Please provide a valid year.");

        var start = new DateTime(targetYear, 1, 1);
        var end = new DateTime(targetYear, 12, 31);

        var holidays = await _unitOfWork.Repository<Holiday>()
            .FindAsync(h => h.HolidayDate >= start && h.HolidayDate <= end);

        return _mapper.Map<List<HolidayResponseDto>>(holidays.OrderBy(h => h.HolidayDate).ToList());
    }

    public async Task<HolidayResponseDto> GetByIdAsync(int id)
    {
        var holiday = await _unitOfWork.Repository<Holiday>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Holiday), id);

        return _mapper.Map<HolidayResponseDto>(holiday);
    }

    public async Task<HolidayResponseDto> CreateAsync(HolidayCreateDto dto)
    {
        var date = dto.HolidayDate.Date;

        if (await _unitOfWork.Repository<Holiday>().AnyAsync(h => h.HolidayDate == date))
            throw new BadRequestException($"A holiday already exists on {date:yyyy-MM-dd}.");

        var holiday = _mapper.Map<Holiday>(dto);
        holiday.HolidayDate = date;

        await _unitOfWork.Repository<Holiday>().AddAsync(holiday);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<HolidayResponseDto>(holiday);
    }

    public async Task<HolidayResponseDto> UpdateAsync(int id, HolidayUpdateDto dto)
    {
        var holiday = await _unitOfWork.Repository<Holiday>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Holiday), id);

        var date = dto.HolidayDate.Date;

        if (await _unitOfWork.Repository<Holiday>().AnyAsync(h => h.HolidayDate == date && h.Id != id))
            throw new BadRequestException($"A holiday already exists on {date:yyyy-MM-dd}.");

        _mapper.Map(dto, holiday);
        holiday.HolidayDate = date;
        holiday.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Holiday>().Update(holiday);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<HolidayResponseDto>(holiday);
    }

    public async Task DeleteAsync(int id)
    {
        var holiday = await _unitOfWork.Repository<Holiday>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Holiday), id);

        _unitOfWork.Repository<Holiday>().Delete(holiday);
        await _unitOfWork.SaveChangesAsync();
    }
}
