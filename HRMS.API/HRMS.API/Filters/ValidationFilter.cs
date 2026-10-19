using FluentValidation;
using HRMS.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRMS.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));

            foreach (var group in result.Errors.GroupBy(e => e.PropertyName))
                errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new ApiResponse<Dictionary<string, string[]>>
            {
                Success = false,
                Message = "Validation failed.",
                Data = errors
            });
            return;
        }

        await next();
    }
}
