using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Files;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class FileService : IFileService
{
    private const int MaxDocumentsPerEmployee = 30;
    private static readonly string[] DocumentTypes = { "CV", "NID", "Certificate", "Other" };

    private readonly IUnitOfWork _uow;
    private readonly IFileStorage _storage;
    private readonly ICurrentUserService _currentUser;

    public FileService(IUnitOfWork uow, IFileStorage storage, ICurrentUserService currentUser)
    {
        _uow = uow;
        _storage = storage;
        _currentUser = currentUser;
    }

    private static string PhotoFolder(int employeeId) => $"employees/{employeeId}/photos";
    private static string DocumentFolder(int employeeId) => $"employees/{employeeId}/documents";
    private static string CvFolder(int applicantId) => $"applicants/{applicantId}/cv";

    // =====================================================================
    // PROFILE PHOTO
    // =====================================================================
    public async Task<UploadedFileDto> UploadPhotoAsync(int employeeId, string? fileName, byte[] content)
    {
        var employee = await LoadEmployeeAsync(employeeId);
        var extension = FileValidator.ValidateAndGetExtension(fileName, content, FilePolicy.Photo);

        var newPath = await _storage.SaveAsync(content, PhotoFolder(employeeId), extension);
        var oldPath = employee.ProfileImage;

        employee.ProfileImage = newPath;
        employee.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Employee>().Update(employee);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch
        {
            await _storage.DeleteAsync(newPath);   // do not leave an orphan file behind
            throw;
        }

        if (FileValidator.IsInsideFolder(oldPath, PhotoFolder(employeeId)))
            await _storage.DeleteAsync(oldPath!);

        return new UploadedFileDto
        {
            Id = employeeId,
            FileName = Path.GetFileName(newPath),
            Size = content.Length,
            UploadedDate = DateTime.UtcNow
        };
    }

    public async Task<FileContentDto> GetPhotoAsync(int employeeId, int? requesterEmployeeId, bool isPrivileged)
    {
        var employee = await LoadEmployeeAsync(employeeId);

        // Somebody else's data is reported as "not found" on purpose.
        if (!isPrivileged && requesterEmployeeId != employeeId)
            throw new NotFoundException(nameof(Employee), employeeId);

        var path = employee.ProfileImage;
        if (!FileValidator.IsInsideFolder(path, PhotoFolder(employeeId)))
            throw new NotFoundException("This employee has no uploaded photo.");

        var bytes = await _storage.ReadAsync(path!) ?? throw new NotFoundException("The photo file was not found.");

        return new FileContentDto
        {
            Content = bytes,
            ContentType = FileValidator.ContentTypeFor(path!),
            FileName = Path.GetFileName(path!)
        };
    }

    // =====================================================================
    // EMPLOYEE DOCUMENTS (CV / NID / Certificate / Other)
    // =====================================================================
    public async Task<UploadedFileDto> UploadDocumentAsync(int employeeId, string? documentType, string? remarks, string? fileName, byte[] content)
    {
        await LoadEmployeeAsync(employeeId);

        var type = DocumentTypes.FirstOrDefault(t => string.Equals(t, documentType?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new BadRequestException("Document Type must be one of: " + string.Join(", ", DocumentTypes) + ".");

        if (remarks != null && remarks.Length > 500)
            throw new BadRequestException("Remarks cannot be longer than 500 characters.");

        var extension = FileValidator.ValidateAndGetExtension(fileName, content, FilePolicy.Document);

        var existing = await _uow.Repository<EmployeeDocument>().FindAsync(d => d.EmployeeId == employeeId);
        if (existing.Count >= MaxDocumentsPerEmployee)
            throw new BadRequestException($"An employee can have a maximum of {MaxDocumentsPerEmployee} documents.");

        var path = await _storage.SaveAsync(content, DocumentFolder(employeeId), extension);

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentType = type,
            FileName = FileValidator.SafeDisplayName(fileName, "document" + extension),
            FilePath = path,
            FileExtension = extension,
            FileSize = content.Length,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = _currentUser.UserId,
            Remarks = remarks?.Trim()
        };

        await _uow.Repository<EmployeeDocument>().AddAsync(document);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch
        {
            await _storage.DeleteAsync(path);
            throw;
        }

        return new UploadedFileDto
        {
            Id = document.Id,
            FileName = document.FileName,
            DocumentType = document.DocumentType,
            Size = content.Length,
            UploadedDate = document.UploadedDate
        };
    }

    public async Task<FileContentDto> GetDocumentAsync(int documentId, int? requesterEmployeeId, bool isPrivileged)
    {
        var document = await _uow.Repository<EmployeeDocument>().GetByIdAsync(documentId)
            ?? throw new NotFoundException(nameof(EmployeeDocument), documentId);

        if (!isPrivileged && requesterEmployeeId != document.EmployeeId)
            throw new NotFoundException(nameof(EmployeeDocument), documentId);

        if (!FileValidator.IsInsideFolder(document.FilePath, DocumentFolder(document.EmployeeId)))
            throw new NotFoundException("The document file was not found.");

        var bytes = await _storage.ReadAsync(document.FilePath) ?? throw new NotFoundException("The document file was not found.");

        return new FileContentDto
        {
            Content = bytes,
            ContentType = FileValidator.ContentTypeFor(document.FilePath),
            FileName = document.FileName
        };
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        var document = await _uow.Repository<EmployeeDocument>().GetByIdAsync(documentId)
            ?? throw new NotFoundException(nameof(EmployeeDocument), documentId);

        var path = document.FilePath;
        var employeeId = document.EmployeeId;

        _uow.Repository<EmployeeDocument>().Delete(document);
        await _uow.SaveChangesAsync();

        if (FileValidator.IsInsideFolder(path, DocumentFolder(employeeId)))
            await _storage.DeleteAsync(path);
    }

    // =====================================================================
    // APPLICANT CV (PDF only)
    // =====================================================================
    public async Task<UploadedFileDto> UploadCvAsync(int applicantId, string? fileName, byte[] content)
    {
        var applicant = await _uow.Repository<Applicant>().GetByIdAsync(applicantId)
            ?? throw new NotFoundException(nameof(Applicant), applicantId);

        var extension = FileValidator.ValidateAndGetExtension(fileName, content, FilePolicy.Cv);

        var newPath = await _storage.SaveAsync(content, CvFolder(applicantId), extension);
        var oldPath = applicant.CvPath;

        applicant.CvPath = newPath;
        applicant.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Applicant>().Update(applicant);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch
        {
            await _storage.DeleteAsync(newPath);
            throw;
        }

        if (FileValidator.IsInsideFolder(oldPath, CvFolder(applicantId)))
            await _storage.DeleteAsync(oldPath!);

        return new UploadedFileDto
        {
            Id = applicantId,
            FileName = FileValidator.SafeDisplayName(fileName, "cv.pdf"),
            Size = content.Length,
            UploadedDate = DateTime.UtcNow
        };
    }

    public async Task<FileContentDto> GetCvAsync(int applicantId)
    {
        var applicant = await _uow.Repository<Applicant>().GetByIdAsync(applicantId)
            ?? throw new NotFoundException(nameof(Applicant), applicantId);

        if (!FileValidator.IsInsideFolder(applicant.CvPath, CvFolder(applicantId)))
            throw new NotFoundException("This applicant has no uploaded CV.");

        var bytes = await _storage.ReadAsync(applicant.CvPath!) ?? throw new NotFoundException("The CV file was not found.");

        return new FileContentDto
        {
            Content = bytes,
            ContentType = "application/pdf",
            FileName = $"{applicant.FullName.Replace(' ', '_')}_CV.pdf"
        };
    }

    private async Task<Employee> LoadEmployeeAsync(int employeeId)
        => await _uow.Repository<Employee>().GetByIdAsync(employeeId) ?? throw new NotFoundException(nameof(Employee), employeeId);
}
