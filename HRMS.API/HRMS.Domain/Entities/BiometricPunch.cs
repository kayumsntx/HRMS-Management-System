using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class BiometricPunch:BaseEntity
    {
        public string FingerprintUserId { get; set; } = default!;
        public DateTime PunchDateTime { get; set; }
        public string? PunchType { get; set; }         // In / Out
        public string? VerificationType { get; set; }
        public string? DeviceTransactionId { get; set; }
        public bool IsProcessed { get; set; } = false;

        public int BiometricDeviceId { get; set; }
        public BiometricDevice BiometricDevice { get; set; } = default!;

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
