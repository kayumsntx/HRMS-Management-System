using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.IdCards;
using HRMS.Domain.Entities;

namespace HRMS.UnitTests.TestSupport;

public class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; set; } = 1;
    public int? EmployeeId { get; set; }
    public string? UserName { get; set; } = "tester";
    public string? IpAddress => "127.0.0.1";
    public bool IsAuthenticated => true;
}

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hash:" + password;
    public bool Verify(string password, string passwordHash) => passwordHash == "hash:" + password;
}

public class FakeJwt : IJwtTokenGenerator
{
    public (string Token, DateTime ExpiresAt) GenerateToken(User user, IEnumerable<string> roles)
        => ("token", DateTime.UtcNow.AddMinutes(15));
}

public class FakeFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task<string> SaveAsync(byte[] content, string folder, string extension)
    {
        var path = $"{folder}/{Guid.NewGuid():N}{extension}";
        _files[path] = content;
        return Task.FromResult(path);
    }

    public Task<byte[]?> ReadAsync(string relativePath)
        => Task.FromResult(_files.TryGetValue(relativePath, out var bytes) ? bytes : null);

    public Task DeleteAsync(string relativePath)
    {
        _files.Remove(relativePath);
        return Task.CompletedTask;
    }
}

public class FakePdfGenerator : IIdCardPdfGenerator
{
    public byte[] GeneratePdf(IdCardPdfModel model) => new byte[] { 0x25, 0x50, 0x44, 0x46 };
    public byte[] GenerateQrPng(string text) => new byte[] { 0x89, 0x50, 0x4E, 0x47 };
}
