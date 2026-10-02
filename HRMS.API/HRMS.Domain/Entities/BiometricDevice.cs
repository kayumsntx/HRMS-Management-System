using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class BiometricDevice
    {
        public string DeviceName { get; set; } = default!;
        public string DeviceSerialNumber { get; set; } = default!;
        public string? DeviceIpAddress { get; set; }
        public int? Port { get; set; }
        public string? LocationName { get; set; }
        public string? DeviceType { get; set; }
        public DateTime? LastSyncDate { get; set; }

        // Navigation
        public ICollection<BiometricPunch> Punches { get; set; } = new List<BiometricPunch>();
    }
}
