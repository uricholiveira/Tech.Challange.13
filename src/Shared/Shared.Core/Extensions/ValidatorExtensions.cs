using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Models.Common;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace Shared.Core.Extensions;

public static class ValidatorExtensions
{
    public static IServiceCollection ConfigureValidators(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(options =>
        {
            options.OverrideDefaultResultFactoryWith<CustomResultFactory>();
        });
        return services;
    }

    private class CustomResultFactory : IFluentValidationAutoValidationResultFactory
    {
        public IActionResult CreateActionResult(ActionExecutingContext context,
            ValidationProblemDetails? validationProblemDetails)
        {
            return new BadRequestObjectResult(new ValidationError("Erro de validação",
                validationProblemDetails?.Errors.Select(x =>
                    new ValidationErrorContent(x.Key, x.Value)).ToList() ?? []));
        }
    }
}