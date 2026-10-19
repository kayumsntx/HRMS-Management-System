using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Users;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        return _mapper.Map<List<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var all = await _unitOfWork.Repository<User>().GetAllAsync();
        if (all.Any(u => string.Equals(u.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"User '{dto.UserName}' already exists.");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _passwordHasher.Hash(dto.Password);

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto dto)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        var all = await _unitOfWork.Repository<User>().GetAllAsync();
        if (all.Any(u => u.Id != id && string.Equals(u.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase)))
            throw new BadRequestException($"User '{dto.UserName}' already exists.");

        _mapper.Map(dto, user);
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        user.IsActive = false;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
