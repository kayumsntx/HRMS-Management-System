using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Employees
{
    public class EmployeeCreateDto
    {
        //public string EmployeeCode { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? NickName { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }

        public DateTime DateOfJoining { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfConfirmation { get; set; }

        public string? NationalIdNo { get; set; }
        public string? TinNumber { get; set; }

        public bool IsOvertimeEligible { get; set; }

        public string? MobileNumber { get; set; }
        public string? FamilyMobileNumber { get; set; }
        public string? Email { get; set; }

        public string? FingerprintUserId { get; set; }
        public string? ProfileImage { get; set; }

        public int EmployeeStatusId { get; set; }
        public int DesignationId { get; set; }
        public int? GradeId { get; set; }
        public int CompanyId { get; set; }
        public int? UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int? SectionId { get; set; }
        public int? ReportingManagerId { get; set; }
    }
}
