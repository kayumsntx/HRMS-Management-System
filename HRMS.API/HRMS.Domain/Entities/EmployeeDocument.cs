using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeDocument:BaseEntity
    {
        public string DocumentType { get; set; } = default!;   // CV, NID, Certificate, Other
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public string? FileExtension { get; set; }
        public long? FileSize { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
        public int? UploadedBy { get; set; }
        public string? Remarks { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}
