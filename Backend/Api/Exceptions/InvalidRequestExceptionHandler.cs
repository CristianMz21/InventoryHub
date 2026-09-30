using InventoryHub.Application.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHub.Api.Exceptions;

public class InvalidRequestExceptionHandler(ILogger<InvalidRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not InvalidRequestException invalid)
            return false;

        logger.LogInformation(invalid, "Request rejected: {Message}", invalid.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = invalid.Message
        }, cancellationToken);

        return true;
    }
}
