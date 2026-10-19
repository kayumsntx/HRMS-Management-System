namespace HRMS.Application.DTOs.Biometric;

public class BiometricDeviceResponseDto
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = default!;
    public string DeviceSerialNumber { get; set; } = default!;
    public string? DeviceIpAddress { get; set; }
    public int? Port { get; set; }
    public string? LocationName { get; set; }
    public string? DeviceType { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
