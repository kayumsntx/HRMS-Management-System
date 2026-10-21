using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Files;
using HRMS.Application.DTOs.IdCards;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class IdCardDocumentService : IIdCardDocumentService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdCardService _idCards;
    private readonly IFileStorage _storage;
    private readonly IIdCardPdfGenerator _generator;

    public IdCardDocumentService(IUnitOfWork uow, IIdCardService idCards, IFileStorage storage, IIdCardPdfGenerator generator)
    {
        _uow = uow;
        _idCards = idCards;
        _storage = storage;
        _generator = generator;
    }

    public async Task<FileContentDto> GetPdfAsync(int idCardId)
    {
        var card = await _uow.Repository<IdCard>().GetByIdAsync(idCardId)
            ?? throw new NotFoundException(nameof(IdCard), idCardId);

        var preview = await _idCards.GetPreviewAsync(idCardId);
        EnsureUsable(preview);

        byte[]? photo = null;
        if (FileValidator.IsInsideFolder(preview.ProfileImage, $"employees/{card.EmployeeId}/photos"))
            photo = await _storage.ReadAsync(preview.ProfileImage!);

        var pdf = _generator.GeneratePdf(new IdCardPdfModel
        {
            CompanyName = preview.CompanyName ?? "",
            CardNo = preview.CardNo,
            FullName = preview.FullName,
            EmployeeCode = preview.EmployeeCode,
            DesignationName = preview.DesignationName,
            DepartmentName = preview.DepartmentName,
            BloodGroup = preview.BloodGroup,
            DateOfJoining = preview.DateOfJoining,
            ExpiryDate = preview.ExpiryDate,
            MobileNumber = preview.MobileNumber,
            EmergencyNumber = preview.FamilyMobileNumber,
            PhotoBytes = photo,
            QrText = QrTextOf(preview)
        });

        return new FileContentDto { Content = pdf, ContentType = "application/pdf", FileName = $"{preview.CardNo}.pdf" };
    }

    public async Task<FileContentDto> GetQrAsync(int idCardId)
    {
        var preview = await _idCards.GetPreviewAsync(idCardId);
        EnsureUsable(preview);

        return new FileContentDto
        {
            Content = _generator.GenerateQrPng(QrTextOf(preview)),
            ContentType = "image/png",
            FileName = $"{preview.CardNo}_qr.png"
        };
    }

    private static string QrTextOf(IdCardPreviewDto preview)
        => string.IsNullOrWhiteSpace(preview.QrCodeData) ? $"HRMS|{preview.EmployeeCode}|{preview.CardNo}" : preview.QrCodeData;

    private static void EnsureUsable(IdCardPreviewDto preview)
    {
        if (preview.CardStatus != IdCardStatus.Active.ToString())
            throw new BadRequestException($"Only an Active card can be generated. Current status: {preview.CardStatus}.");

        if (preview.IsExpired)
            throw new BadRequestException("This card has expired. Please re-issue it first.");
    }
}
