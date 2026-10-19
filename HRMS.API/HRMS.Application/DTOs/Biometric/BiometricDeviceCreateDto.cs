namespace HRMS.Application.DTOs.Biometric;

public class BiometricDeviceCreateDto
{
    public string DeviceName { get; set; } = default!;
    public string DeviceSerialNumber { get; set; } = default!;
    public string? DeviceIpAddress { get; set; }
    public int? Port { get; set; }
    public string? LocationName { get; set; }
    public string? DeviceType { get; set; }
}
