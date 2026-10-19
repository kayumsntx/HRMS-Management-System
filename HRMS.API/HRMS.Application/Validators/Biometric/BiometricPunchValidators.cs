using FluentValidation;
using HRMS.Application.DTOs.Biometric;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Biometric;

public class BiometricPunchCreateValidator : AbstractValidator<BiometricPunchCreateDto>
{
    public BiometricPunchCreateValidator()
    {
        RuleFor(x => x.BiometricDeviceId).GreaterThan(0).WithMessage("Please select a valid Biometric Device.");

        RuleFor(x => x.FingerprintUserId)
            .NotEmpty().WithMessage("Fingerprint User Id is required.")
            .MaximumLength(50);

        RuleFor(x => x.PunchDateTime)
            .NotEmpty().WithMessage("Punch Date Time is required.")
            .Must(d => d <= DateTime.Now.AddDays(1)).WithMessage("Punch Date Time cannot be in the future.");

        RuleFor(x => x.PunchType)
            .Must(v => Enum.TryParse<PunchType>(v, true, out var parsed) && Enum.IsDefined(typeof(PunchType), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.PunchType))
            .WithMessage("Punch Type must be 'In' or 'Out'.");

        RuleFor(x => x.VerificationType).MaximumLength(30);
        RuleFor(x => x.DeviceTransactionId).MaximumLength(100);
    }
}

public class BiometricPunchBulkCreateValidator : AbstractValidator<BiometricPunchBulkCreateDto>
{
    public BiometricPunchBulkCreateValidator()
    {
        RuleFor(x => x.Punches)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("At least one punch is required.")
            .Must(p => p.Count <= 5000).WithMessage("A maximum of 5000 punches per request is allowed.");

        RuleForEach(x => x.Punches).SetValidator(new BiometricPunchCreateValidator());
    }
}
