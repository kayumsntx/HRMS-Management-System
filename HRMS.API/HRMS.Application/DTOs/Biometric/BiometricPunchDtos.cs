namespace HRMS.Application.DTOs.Biometric;

public class BiometricPunchCreateDto
{
    public int BiometricDeviceId { get; set; }
    public string FingerprintUserId { get; set; } = default!;
    public DateTime PunchDateTime { get; set; }
    public string? PunchType { get; set; }          // In / Out (optional)
    public string? VerificationType { get; set; }   // Fingerprint / Face / Card ...
    public string? DeviceTransactionId { get; set; }
}

public class BiometricPunchBulkCreateDto
{
    public List<BiometricPunchCreateDto> Punches { get; set; } = new();
}

public class BiometricPunchBulkResultDto
{
    public int Total { get; set; }
    public int Inserted { get; set; }
    public int Duplicates { get; set; }
    public int InvalidDevice { get; set; }
    public int UnmappedEmployee { get; set; }
}

public class BiometricPunchResponseDto
{
    public int Id { get; set; }
    public int BiometricDeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string FingerprintUserId { get; set; } = default!;
    public int? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime PunchDateTime { get; set; }
    public string? PunchType { get; set; }
    public string? VerificationType { get; set; }
    public string? DeviceTransactionId { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime CreatedDate { get; set; }
}
