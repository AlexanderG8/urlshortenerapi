using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace urlshortenerapi.Infraestructure.ErrorHandling
{
    internal sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService
        ,ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
            )
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Petición cancelada por el cliente: {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
                return true;
            }

            logger.LogError(exception, "Excepción no controlada en {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Detail = environment.IsDevelopment() ? exception.Message : "Ha ocurrido un error inesperado. Por favor, inténtelo de nuevo más tarde."
                };
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }
    }
}
