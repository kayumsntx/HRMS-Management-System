namespace HRMS.Application.DTOs.Files;

public class UploadedFileDto
{
    public int Id { get; set; }                  // employee id (photo / CV) or document id
    public string FileName { get; set; } = default!;
    public string? DocumentType { get; set; }
    public long Size { get; set; }
    public DateTime UploadedDate { get; set; }
}

public class FileContentDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = default!;
}
