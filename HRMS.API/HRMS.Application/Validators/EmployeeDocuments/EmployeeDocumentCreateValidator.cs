using FluentValidation;
using HRMS.Application.DTOs.EmployeeDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.EmployeeDocuments
{
    public class EmployeeDocumentCreateValidator : AbstractValidator<EmployeeDocumentCreateDto>
    {
        private static readonly string[] AllowedTypes = { "CV", "NID", "Certificate", "Other" };

        public EmployeeDocumentCreateValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Please select a valid Employee.");

            RuleFor(x => x.DocumentType)
                .NotEmpty().WithMessage("Document Type is required.")
                .Must(type => AllowedTypes.Contains(type))
                .WithMessage($"Document Type must be one of: {string.Join(", ", AllowedTypes)}.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File Name is required.")
                .MaximumLength(255);

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("File Path is required.")
                .MaximumLength(500);

            RuleFor(x => x.FileExtension)
                .MaximumLength(20);
        }
    }
}
