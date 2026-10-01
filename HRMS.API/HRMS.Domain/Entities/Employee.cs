using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Employee:BaseEntity
    {
        public string EmployeeCode { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? NickName { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }

        public DateTime DateOfJoining { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfConfirmation { get; set; }

        public string? NationalIdNo { get; set; }
        public string? TinNumber { get; set; }

        public bool IsOvertimeEligible { get; set; } = false;

        public string? MobileNumber { get; set; }
        public string? FamilyMobileNumber { get; set; }
        public string? Email { get; set; }

        public string? FingerprintUserId { get; set; }
        public string? ProfileImage { get; set; }

        // ---- Foreign Keys ----
        public int EmployeeStatusId { get; set; }
        public EmployeeStatus EmployeeStatus { get; set; } = default!;

        public int DesignationId { get; set; }
        public Designation Designation { get; set; } = default!;

        public int? GradeId { get; set; }
        public Grade? Grade { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = default!;

        public int? SectionId { get; set; }
        public Section? Section { get; set; }

        // Self-reference — Reporting Manager
        public int? ReportingManagerId { get; set; }
        public Employee? ReportingManager { get; set; }

        // ---- Navigation (Child collections) ----
        public ICollection<EmployeeEducation> Educations { get; set; } = new List<EmployeeEducation>();
        public ICollection<EmployeeNominee> Nominees { get; set; } = new List<EmployeeNominee>();
        public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    }
}
