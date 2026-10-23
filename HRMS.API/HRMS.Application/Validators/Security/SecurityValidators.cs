using FluentValidation;
using HRMS.Application.DTOs.Security;

namespace HRMS.Application.Validators.Security;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required.").MaximumLength(200);
    }
}

public class LoginHistoryFilterValidator : AbstractValidator<LoginHistoryFilterDto>
{
    public LoginHistoryFilterValidator()
    {
        RuleFor(x => x.UserName).MaximumLength(100);
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate.Value.Date >= x.FromDate.Value.Date)
            .WithMessage("To Date must be on or after From Date.");
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || (x.ToDate.Value.Date - x.FromDate.Value.Date).TotalDays <= 31)
            .WithMessage("A maximum of 31 days can be searched at a time.");
    }
}

public abstract class MenuRules<T> : AbstractValidator<T> where T : MenuCreateDto
{
    protected MenuRules()
    {
        RuleFor(x => x.ParentMenuId).GreaterThan(0).When(x => x.ParentMenuId.HasValue);
        RuleFor(x => x.MenuName).NotEmpty().WithMessage("Menu Name is required.").MaximumLength(100);
        RuleFor(x => x.MenuUrl).MaximumLength(200)
            .Must(u => u!.StartsWith("/") && !u.Contains(' ')).When(x => !string.IsNullOrWhiteSpace(x.MenuUrl))
            .WithMessage("Menu Url must start with '/' and contain no spaces.");
        RuleFor(x => x.IconClass).MaximumLength(100);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 10000);
    }
}
public class MenuCreateValidator : MenuRules<MenuCreateDto> { }
public class MenuUpdateValidator : MenuRules<MenuUpdateDto> { }

public class MenuPermissionSaveValidator : AbstractValidator<MenuPermissionSaveDto>
{
    public MenuPermissionSaveValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .Must(l => l == null || l.Count <= 500).WithMessage("A maximum of 500 menu items is allowed.")
            .Must(l => l == null || l.Select(i => i.MenuId).Distinct().Count() == l.Count).WithMessage("A menu cannot be listed twice.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuId).GreaterThan(0);
            item.RuleFor(i => i).Must(i => i.CanView || !(i.CanCreate || i.CanEdit || i.CanDelete))
                .WithMessage("Create, Edit and Delete require View access.");
        });
    }
}
