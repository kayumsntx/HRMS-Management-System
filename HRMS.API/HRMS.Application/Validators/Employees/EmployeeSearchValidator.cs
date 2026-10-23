using FluentValidation;
using HRMS.Application.DTOs.Employees;

namespace HRMS.Application.Validators.Employees;

public class EmployeeSearchValidator : AbstractValidator<EmployeeSearchDto>
{
    public EmployeeSearchValidator()
    {
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Page Number must be at least 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Page Size must be between 1 and 100.");
    }
}
