using FluentValidation;
using HRMS.Application.DTOs.Holidays;

namespace HRMS.Application.Validators.Holidays;

public class HolidayCreateValidator : AbstractValidator<HolidayCreateDto>
{
    public HolidayCreateValidator()
    {
        RuleFor(x => x.HolidayDate).NotEmpty().WithMessage("Holiday Date is required.");
        RuleFor(x => x.HolidayName).NotEmpty().WithMessage("Holiday Name is required.").MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public class HolidayUpdateValidator : AbstractValidator<HolidayUpdateDto>
{
    public HolidayUpdateValidator()
    {
        RuleFor(x => x.HolidayDate).NotEmpty().WithMessage("Holiday Date is required.");
        RuleFor(x => x.HolidayName).NotEmpty().WithMessage("Holiday Name is required.").MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}
