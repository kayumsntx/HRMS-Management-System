using FluentValidation;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Career;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Career;

public class CareerChangeCreateValidator : AbstractValidator<CareerChangeCreateDto>
{
    public CareerChangeCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.ChangeType).Must(v => EnumHelper.IsValid<CareerActionType>(v))
            .WithMessage("Invalid Change Type. Allowed: Promotion, Transfer, DepartmentChange, DesignationChange, GradeChange.");
        RuleFor(x => x.EffectiveDate).NotEmpty().WithMessage("Effective Date is required.");
        RuleFor(x => x.NewDesignationId).GreaterThan(0).When(x => x.NewDesignationId.HasValue);
        RuleFor(x => x.NewGradeId).GreaterThan(0).When(x => x.NewGradeId.HasValue);
        RuleFor(x => x.NewDepartmentId).GreaterThan(0).When(x => x.NewDepartmentId.HasValue);
        RuleFor(x => x.NewUnitId).GreaterThan(0).When(x => x.NewUnitId.HasValue);
        RuleFor(x => x.NewSectionId).GreaterThan(0).When(x => x.NewSectionId.HasValue);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}
