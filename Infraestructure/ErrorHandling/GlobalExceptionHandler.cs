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
    CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Petición cancelada por el cliente: {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
                return true;
            }

            ProblemDetails problemDetails;

            if (exception is BadHttpRequestException badRequest)
            {
                // Error del cliente (parámetro con formato inválido, JSON mal formado): no es un error del servidor.
                logger.LogWarning("Solicitud inválida en {Method} {Path}: {Message}",
                    httpContext.Request.Method, httpContext.Request.Path, badRequest.Message);

                problemDetails = new ProblemDetails
                {
                    Status = badRequest.StatusCode,
                    Title = "Solicitud inválida",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Detail = badRequest.Message // describe la entrada del cliente, no la estructura interna
                };
            }
            else
            {
                logger.LogError(exception, "Excepción no controlada en {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);

                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Detail = environment.IsDevelopment() ? exception.Message : "Ocurrió un error inesperado."
                };
            }

            httpContext.Response.StatusCode = problemDetails.Status!.Value;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }
    }
}
