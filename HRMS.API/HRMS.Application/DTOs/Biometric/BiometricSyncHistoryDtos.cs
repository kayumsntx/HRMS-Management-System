namespace HRMS.Application.DTOs.Biometric;

public class BiometricSyncHistoryResponseDto
{
    public int Id { get; set; }
    public int BiometricDeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string SyncType { get; set; } = default!;
    public DateTime StartTime { get; set; }     // UTC
    public DateTime? EndTime { get; set; }      // UTC
    public int TotalRecords { get; set; }
    public int NewRecords { get; set; }
    public string Status { get; set; } = default!;
    public string? ErrorMessage { get; set; }
}
