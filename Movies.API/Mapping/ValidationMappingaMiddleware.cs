using FluentValidation;
using Movies.Contracts.Responses;

namespace Movies.API.Mapping;

public class ValidationMappingaMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMappingaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var validationfailuereResponse = new ValidationFailureResponse
            {
                Errros = e.Errors.Select(error => new ValidationResponse
                {
                    PropertyName = error.PropertyName,
                    Message = error.ErrorMessage
                })
            };
            
            await context.Response.WriteAsJsonAsync(validationfailuereResponse);
        }
    }
}