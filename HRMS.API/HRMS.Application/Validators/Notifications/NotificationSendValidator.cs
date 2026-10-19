using FluentValidation;
using HRMS.Application.DTOs.Notifications;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Notifications;

public class NotificationSendValidator : AbstractValidator<NotificationSendDto>
{
    public NotificationSendValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).When(x => x.UserId.HasValue).WithMessage("Please provide a valid User.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message is required.").MaximumLength(1000);
        RuleFor(x => x.LinkUrl).MaximumLength(300);

        RuleFor(x => x.NotificationType)
            .Must(v => Enum.TryParse<NotificationCategory>(v, true, out var parsed) && Enum.IsDefined(typeof(NotificationCategory), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.NotificationType))
            .WithMessage("Invalid Notification Type. Allowed: General, Leave, Attendance, Payroll, Approval.");
    }
}
