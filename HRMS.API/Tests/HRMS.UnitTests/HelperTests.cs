using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.UnitTests;

public class FileValidatorTests
{
    private static byte[] Jpeg(int size = 100) { var b = new byte[size]; b[0] = 0xFF; b[1] = 0xD8; b[2] = 0xFF; return b; }
    private static byte[] Pdf(int size = 100) { var b = new byte[size]; b[0] = 0x25; b[1] = 0x50; b[2] = 0x44; b[3] = 0x46; return b; }

    [Fact]
    public void Valid_jpeg_is_accepted()
        => Assert.Equal(".jpg", FileValidator.ValidateAndGetExtension("me.jpg", Jpeg(), FilePolicy.Photo));

    [Fact]
    public void Jpeg_extension_is_normalized()
        => Assert.Equal(".jpg", FileValidator.ValidateAndGetExtension("ME.JPEG", Jpeg(), FilePolicy.Photo));

    [Fact]
    public void Renamed_executable_is_rejected()
    {
        var exe = new byte[100];
        exe[0] = 0x4D; exe[1] = 0x5A;   // "MZ" header of a Windows program
        Assert.Throws<BadRequestException>(() => FileValidator.ValidateAndGetExtension("photo.jpg", exe, FilePolicy.Photo));
    }

    [Fact]
    public void Oversized_file_is_rejected()
        => Assert.Throws<BadRequestException>(() => FileValidator.ValidateAndGetExtension("big.jpg", Jpeg(3 * 1024 * 1024), FilePolicy.Photo));

    [Fact]
    public void Empty_file_is_rejected()
        => Assert.Throws<BadRequestException>(() => FileValidator.ValidateAndGetExtension("a.jpg", Array.Empty<byte>(), FilePolicy.Photo));

    [Fact]
    public void Pdf_is_not_allowed_as_a_photo()
        => Assert.Throws<BadRequestException>(() => FileValidator.ValidateAndGetExtension("a.pdf", Pdf(), FilePolicy.Photo));

    [Fact]
    public void Stored_path_must_stay_inside_its_folder()
    {
        Assert.True(FileValidator.IsInsideFolder("employees/5/photos/a.jpg", "employees/5/photos"));
        Assert.False(FileValidator.IsInsideFolder("employees/6/photos/a.jpg", "employees/5/photos"));
        Assert.False(FileValidator.IsInsideFolder("employees/5/photos/../x.jpg", "employees/5/photos"));
        Assert.False(FileValidator.IsInsideFolder(null, "employees/5/photos"));
    }
}

public class NumberAndEnumHelperTests
{
    [Fact]
    public void Next_number_continues_after_the_highest_one()
        => Assert.Equal("RQ000008", NumberHelper.Next("RQ", new[] { "RQ000007", "RQ000002", "XX1" }, 6));

    [Fact]
    public void Next_number_starts_at_one()
        => Assert.Equal("RQ000001", NumberHelper.Next("RQ", Array.Empty<string>(), 6));

    [Fact]
    public void Enum_names_are_normalized()
        => Assert.Equal("Approved", EnumHelper.Normalize<LeaveStatus>("approved", "status"));

    [Fact]
    public void Unknown_enum_text_is_rejected()
        => Assert.Throws<BadRequestException>(() => EnumHelper.Normalize<LeaveStatus>("nonsense", "status"));

    [Fact]
    public void Numbers_are_not_accepted_as_enum_names()
        => Assert.False(EnumHelper.IsValid<LeaveStatus>("123"));
}

public class LoanTests
{
    [Fact]
    public void Last_installment_absorbs_the_rounding_difference()
    {
        var loan = new Loan { LoanAmount = 10000m, TotalInstallments = 3, InstallmentAmount = 3333.33m };

        Assert.Equal(3333.33m, LoanService.InstallmentFor(loan, 1));
        Assert.Equal(3333.33m, LoanService.InstallmentFor(loan, 2));
        Assert.Equal(3333.34m, LoanService.InstallmentFor(loan, 3));
        Assert.Equal(loan.LoanAmount, LoanService.InstallmentFor(loan, 1) + LoanService.InstallmentFor(loan, 2) + LoanService.InstallmentFor(loan, 3));
    }
}
