using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string HR = "HR";
            public const string Manager = "Manager";
            public const string Employee = "Employee";
        }

        public static class Pagination
        {
            public const int DefaultPageNumber = 1;
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 100;
        }

        public static class FileUpload
        {
            public const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
            public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        }
    }
}
