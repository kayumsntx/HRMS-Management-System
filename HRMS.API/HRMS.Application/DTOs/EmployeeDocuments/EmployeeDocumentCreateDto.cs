using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.EmployeeDocuments
{
    public class EmployeeDocumentCreateDto
    {
        public int EmployeeId { get; set; }
        public string DocumentType { get; set; } = default!;   // CV, NID, Certificate, Other
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public string? FileExtension { get; set; }
        public long? FileSize { get; set; }
        public string? Remarks { get; set; }
    }
}
