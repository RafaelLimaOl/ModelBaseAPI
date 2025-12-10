using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ModelBaseAPI.Utilities;

[Serializable]
public class ProblemExeption(string error, string message, int statusCode) : Exception(message)
{
    public string Error { get; } = error;
    public string DetailMessage { get; } = message;
    public int StatusCode { get; } = statusCode;
}

public class ProblemExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{

    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ProblemExeption problemExeption)
        {
            return true;
        }

        var problemDetails = new ProblemDetails
        {
            Status = problemExeption.StatusCode,
            Title = problemExeption.Error,
            Detail = problemExeption.Message,
            Type = GetProblemType(problemExeption.StatusCode)
        };

        httpContext.Response.StatusCode = problemExeption.StatusCode;
        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            }
        );
    }

    private static string GetProblemType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "Unknown Error"
    };
}

// Use Exemple: throw new ProblemExeption("Unauthorized Access", "You are not authorized to perform this action.", StatusCodes.Status401Unauthorized);