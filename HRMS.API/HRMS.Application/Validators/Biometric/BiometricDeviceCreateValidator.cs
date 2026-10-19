using System.Net;
using FluentValidation;
using HRMS.Application.DTOs.Biometric;

namespace HRMS.Application.Validators.Biometric;

public class BiometricDeviceCreateValidator : AbstractValidator<BiometricDeviceCreateDto>
{
    public BiometricDeviceCreateValidator()
    {
        RuleFor(x => x.DeviceName).NotEmpty().WithMessage("Device Name is required.").MaximumLength(100);
        RuleFor(x => x.DeviceSerialNumber).NotEmpty().WithMessage("Device Serial Number is required.").MaximumLength(100);
        RuleFor(x => x.DeviceIpAddress)
            .Must(ip => IPAddress.TryParse(ip, out _))
            .When(x => !string.IsNullOrEmpty(x.DeviceIpAddress))
            .WithMessage("Please provide a valid IP address.");
        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .When(x => x.Port.HasValue)
            .WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.LocationName).MaximumLength(150);
        RuleFor(x => x.DeviceType).MaximumLength(50);
    }
}
