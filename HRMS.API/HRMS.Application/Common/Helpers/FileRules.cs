using HRMS.Application.Common.Exceptions;

namespace HRMS.Application.Common.Helpers;

public sealed class FilePolicy
{
    public string[] AllowedExtensions { get; init; } = Array.Empty<string>();
    public long MaxBytes { get; init; }

    public static readonly FilePolicy Photo = new()
    {
        AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" },
        MaxBytes = 2 * 1024 * 1024
    };

    public static readonly FilePolicy Document = new()
    {
        AllowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" },
        MaxBytes = 5 * 1024 * 1024
    };

    public static readonly FilePolicy Cv = new()
    {
        AllowedExtensions = new[] { ".pdf" },
        MaxBytes = 5 * 1024 * 1024
    };
}

public static class FileValidator
{
    // Checks size, extension AND the real file signature (so "virus.exe" renamed to "photo.jpg" is rejected).
    public static string ValidateAndGetExtension(string? fileName, byte[] content, FilePolicy policy)
    {
        if (content.Length == 0)
            throw new BadRequestException("The file is empty.");

        if (content.Length > policy.MaxBytes)
            throw new BadRequestException($"The file is too large. Maximum allowed size is {policy.MaxBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

        if (!policy.AllowedExtensions.Contains(extension))
            throw new BadRequestException($"File type not allowed. Allowed types: {string.Join(", ", policy.AllowedExtensions)}.");

        if (!SignatureMatches(extension, content))
            throw new BadRequestException("The file content does not match its extension.");

        return extension == ".jpeg" ? ".jpg" : extension;
    }

    public static string ContentTypeFor(string pathOrName)
        => Path.GetExtension(pathOrName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

    public static string SafeDisplayName(string? fileName, string fallback)
    {
        var name = new string((fileName ?? string.Empty).Where(c => !char.IsControl(c) && c != '/' && c != '\\').ToArray()).Trim();
        if (name.Length == 0)
            name = fallback;

        return name.Length > 255 ? name.Substring(name.Length - 255) : name;   // keep the extension
    }

    // A stored path is trusted only if it lives inside the expected folder.
    public static bool IsInsideFolder(string? path, string folder)
        => !string.IsNullOrWhiteSpace(path) && !path.Contains("..") && path.StartsWith(folder + "/", StringComparison.Ordinal);

    private static bool SignatureMatches(string extension, byte[] c)
    {
        static bool Starts(byte[] data, params byte[] signature)
            => data.Length >= signature.Length && data.Take(signature.Length).SequenceEqual(signature);

        return extension switch
        {
            ".pdf" => Starts(c, 0x25, 0x50, 0x44, 0x46),                              // %PDF
            ".jpg" or ".jpeg" => Starts(c, 0xFF, 0xD8, 0xFF),
            ".png" => Starts(c, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
            _ => false
        };
    }
}
