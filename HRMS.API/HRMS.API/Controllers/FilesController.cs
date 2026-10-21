using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Files;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

public class FileUploadForm
{
    public IFormFile? File { get; set; }
}

public class DocumentUploadForm
{
    public IFormFile? File { get; set; }
    public string? DocumentType { get; set; }   // CV, NID, Certificate, Other
    public string? Remarks { get; set; }
}

// Everyone logged in can reach this controller; each action decides who may do what.
[Authorize]
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileService _service;
    private readonly ICurrentUserService _currentUser;

    public FilesController(IFileService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private bool IsPrivileged => User.IsInRole("Admin") || User.IsInRole("HR");

    private static async Task<(string Name, byte[] Content)> ReadFileAsync(IFormFile? file, long maxBytes)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("Please select a file.");

        if (file.Length > maxBytes)
            throw new BadRequestException($"The file is too large. Maximum allowed size is {maxBytes / (1024 * 1024)} MB.");

        using var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream);
        return (file.FileName, stream.ToArray());
    }

    private IActionResult Download(FileContentDto file, bool inline)
    {
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return inline ? File(file.Content, file.ContentType) : File(file.Content, file.ContentType, file.FileName);
    }

    // ---------------- Profile photo ----------------

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("employees/{employeeId:int}/photo")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(int employeeId, [FromForm] FileUploadForm form)
    {
        var (name, content) = await ReadFileAsync(form.File, FilePolicy.Photo.MaxBytes);
        var result = await _service.UploadPhotoAsync(employeeId, name, content);
        return Ok(ApiResponse<UploadedFileDto>.SuccessResponse(result, "Photo uploaded successfully."));
    }

    // Admin/HR can open any photo; an employee only his/her own.
    [HttpGet("employees/{employeeId:int}/photo")]
    public async Task<IActionResult> GetPhoto(int employeeId)
        => Download(await _service.GetPhotoAsync(employeeId, _currentUser.EmployeeId, IsPrivileged), inline: true);

    // ---------------- Employee documents ----------------

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("employees/{employeeId:int}/documents")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(int employeeId, [FromForm] DocumentUploadForm form)
    {
        var (name, content) = await ReadFileAsync(form.File, FilePolicy.Document.MaxBytes);
        var result = await _service.UploadDocumentAsync(employeeId, form.DocumentType, form.Remarks, name, content);
        return Ok(ApiResponse<UploadedFileDto>.SuccessResponse(result, "Document uploaded successfully."));
    }

    [HttpGet("documents/{id:int}")]
    public async Task<IActionResult> GetDocument(int id)
        => Download(await _service.GetDocumentAsync(id, _currentUser.EmployeeId, IsPrivileged), inline: false);

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("documents/{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        await _service.DeleteDocumentAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Document deleted successfully."));
    }

    // ---------------- Applicant CV ----------------

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("applicants/{applicantId:int}/cv")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadCv(int applicantId, [FromForm] FileUploadForm form)
    {
        var (name, content) = await ReadFileAsync(form.File, FilePolicy.Cv.MaxBytes);
        var result = await _service.UploadCvAsync(applicantId, name, content);
        return Ok(ApiResponse<UploadedFileDto>.SuccessResponse(result, "CV uploaded successfully."));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpGet("applicants/{applicantId:int}/cv")]
    public async Task<IActionResult> GetCv(int applicantId)
        => Download(await _service.GetCvAsync(applicantId), inline: false);
}
