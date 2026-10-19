using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.IdCards;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class IdCardService : IIdCardService
{
    private const int DefaultValidityYears = 3;   // card validity when no expiry date is given
    private const string CardNoPrefix = "IC";

    private static readonly string StatusActive = IdCardStatus.Active.ToString();
    private static readonly string StatusLost = IdCardStatus.Lost.ToString();
    private static readonly string StatusCancelled = IdCardStatus.Cancelled.ToString();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public IdCardService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    // =====================================================================
    // QUERY
    // =====================================================================
    public async Task<List<IdCardResponseDto>> GetAllAsync(string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : ParseStatus(status);

        var cards = await _unitOfWork.Repository<IdCard>()
            .FindAsync(c => statusText == null || c.CardStatus == statusText);

        return await MapAsync(cards.OrderByDescending(c => c.IssueDate).ThenByDescending(c => c.Id));
    }

    public async Task<List<IdCardResponseDto>> GetByEmployeeIdAsync(int employeeId)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var cards = await _unitOfWork.Repository<IdCard>().FindAsync(c => c.EmployeeId == employeeId);
        return await MapAsync(cards.OrderByDescending(c => c.IssueDate).ThenByDescending(c => c.Id));
    }

    public async Task<IdCardResponseDto> GetByIdAsync(int id)
    {
        var card = await _unitOfWork.Repository<IdCard>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(IdCard), id);

        return (await MapAsync(new[] { card })).First();
    }

    public async Task<IdCardPreviewDto> GetPreviewAsync(int id)
    {
        var card = await _unitOfWork.Repository<IdCard>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(IdCard), id);

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(card.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), card.EmployeeId);

        var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(employee.DesignationId);
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(employee.DepartmentId);
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(employee.CompanyId);

        return new IdCardPreviewDto
        {
            IdCardId = card.Id,
            CardNo = card.CardNo,
            QrCodeData = card.QrCodeData,
            CardStatus = card.CardStatus,
            IssueDate = card.IssueDate,
            ExpiryDate = card.ExpiryDate,
            IsExpired = IsExpired(card),

            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            DesignationName = designation?.DesignationName,
            DepartmentName = department?.DepartmentName,
            CompanyName = company?.CompanyName,
            BloodGroup = employee.BloodGroup,
            DateOfJoining = employee.DateOfJoining,
            MobileNumber = employee.MobileNumber,
            FamilyMobileNumber = employee.FamilyMobileNumber,
            ProfileImage = employee.ProfileImage
        };
    }

    // =====================================================================
    // ISSUE / RE-ISSUE
    // =====================================================================
    public async Task<IdCardResponseDto> IssueAsync(IdCardIssueDto dto)
    {
        var employee = await GetActiveEmployeeAsync(dto.EmployeeId);

        if (await _unitOfWork.Repository<IdCard>().AnyAsync(c => c.EmployeeId == dto.EmployeeId && c.CardStatus == StatusActive))
            throw new BadRequestException("This Employee already has an active ID card. Use re-issue to replace it.");

        return await CreateCardAsync(employee, IdCardIssueType.Issue, dto.ExpiryDate, null);
    }

    public async Task<IdCardResponseDto> ReissueAsync(IdCardReissueDto dto)
    {
        var employee = await GetActiveEmployeeAsync(dto.EmployeeId);

        var activeCard = (await _unitOfWork.Repository<IdCard>()
                .FindAsync(c => c.EmployeeId == dto.EmployeeId && c.CardStatus == StatusActive))
            .FirstOrDefault()
            ?? throw new BadRequestException("This Employee has no active ID card. Please issue a new card instead.");

        // Step 1: retire the old card and save first (the database allows only one Active card per employee).
        activeCard.CardStatus = dto.IsLost ? StatusLost : StatusCancelled;
        activeCard.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Repository<IdCard>().Update(activeCard);
        await _unitOfWork.SaveChangesAsync();

        // Step 2: create the replacement card.
        return await CreateCardAsync(employee, IdCardIssueType.ReIssue, dto.ExpiryDate, dto.Reason.Trim());
    }

    // =====================================================================
    // STATUS / PRINT
    // =====================================================================
    public async Task<IdCardResponseDto> ChangeStatusAsync(int id, IdCardStatusDto dto)
    {
        var card = await _unitOfWork.Repository<IdCard>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(IdCard), id);

        var target = ParseStatus(dto.Status);

        if (target == StatusActive)
            throw new BadRequestException("An ID card cannot be re-activated. Please issue a new card.");

        if (card.CardStatus != StatusActive)
            throw new BadRequestException($"Only an Active card can be changed. Current status: {card.CardStatus}.");

        card.CardStatus = target;
        card.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<IdCard>().Update(card);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { card })).First();
    }

    public async Task<IdCardResponseDto> RegisterPrintAsync(int id)
    {
        var card = await _unitOfWork.Repository<IdCard>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(IdCard), id);

        if (card.CardStatus != StatusActive)
            throw new BadRequestException($"Only an Active card can be printed. Current status: {card.CardStatus}.");

        if (IsExpired(card))
            throw new BadRequestException("This card has expired. Please re-issue it before printing.");

        card.PrintCount += 1;
        card.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<IdCard>().Update(card);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { card })).First();
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================
    private async Task<Employee> GetActiveEmployeeAsync(int employeeId)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        if (!employee.IsActive)
            throw new BadRequestException("An ID card cannot be issued to an inactive employee.");

        return employee;
    }

    private async Task<IdCardResponseDto> CreateCardAsync(Employee employee, IdCardIssueType issueType, DateTime? expiryDate, string? reason)
    {
        var issueDate = DateTime.Now.Date;
        var expiry = expiryDate?.Date ?? issueDate.AddYears(DefaultValidityYears);

        if (expiry <= issueDate)
            throw new BadRequestException("Expiry Date must be after the issue date.");

        var cardNo = await GenerateCardNoAsync();

        var card = new IdCard
        {
            EmployeeId = employee.Id,
            CardNo = cardNo,
            IssueType = issueType.ToString(),
            IssueDate = issueDate,
            ExpiryDate = expiry,
            ReIssueReason = reason,
            QrCodeData = $"HRMS|{employee.EmployeeCode}|{cardNo}",
            CardStatus = StatusActive,
            IssuedBy = _currentUser.UserId
        };

        await _unitOfWork.Repository<IdCard>().AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { card })).First();
    }

    private async Task<string> GenerateCardNoAsync()
    {
        var cards = await _unitOfWork.Repository<IdCard>().GetAllAsync();

        var maxNumber = cards
            .Select(c => c.CardNo)
            .Where(n => n.StartsWith(CardNoPrefix) && int.TryParse(n.Substring(CardNoPrefix.Length), out _))
            .Select(n => int.Parse(n.Substring(CardNoPrefix.Length)))
            .DefaultIfEmpty(0)
            .Max();

        return $"{CardNoPrefix}{maxNumber + 1:D6}";   // IC000001, IC000002 ...
    }

    private static bool IsExpired(IdCard card)
        => card.ExpiryDate.HasValue && card.ExpiryDate.Value.Date < DateTime.Now.Date;

    private static string ParseStatus(string value)
    {
        if (Enum.TryParse<IdCardStatus>(value, true, out var parsed) && Enum.IsDefined(typeof(IdCardStatus), parsed))
            return parsed.ToString();

        throw new BadRequestException("Invalid status. Allowed: " + string.Join(", ", Enum.GetNames<IdCardStatus>()) + ".");
    }

    private async Task<List<IdCardResponseDto>> MapAsync(IEnumerable<IdCard> cards)
    {
        var list = cards.ToList();
        var employeeIds = list.Select(c => c.EmployeeId).Distinct().ToList();

        var employees = employeeIds.Count == 0
            ? new Dictionary<int, Employee>()
            : (await _unitOfWork.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id))).ToDictionary(e => e.Id);

        var today = DateTime.Now.Date;
        var result = _mapper.Map<List<IdCardResponseDto>>(list);

        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }

            dto.IsExpired = dto.ExpiryDate.HasValue && dto.ExpiryDate.Value.Date < today;
        }

        return result;
    }
}
