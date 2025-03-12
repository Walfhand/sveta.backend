using Engine.Exceptions;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Engine.ProblemDetails;

public static class ProblemDetailsConfig
{
    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails(options =>
        {
            options.IncludeExceptionDetails = (ctx, _) => false;
            options.Map<ValidationException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = ex.Message,
                Status = 400,
                Detail = string.Join(',', ex.Errors.Select(failure => failure.ErrorMessage))
            });
            options.Map<CustomException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = $"A {ex.GetType()} error has occurred",
                Status = (int)ex.StatusCode,
                Detail = ex.Message
            });
            options.MapToStatusCode<ArgumentNullException>(StatusCodes.Status400BadRequest);
        });
        return services;
    }
}