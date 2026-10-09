using Scalar.AspNetCore;
using urlshortenerapi.Infraestructure.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Metadatos del documento OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Url Shortener API";
        document.Info.Version = "v1";
        document.Info.Description = "API para acortar URLs y redirigir a la URL original.";
        return Task.CompletedTask;
    });
});
// Configuración de ProblemDetails para personalizar la respuesta de error
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
    };
});
// Agrega el manejador de excepciones global
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// El orden importa: el manejador de excepciones debe ir primero para capturar todo lo que viene después.
app.UseExceptionHandler();
app.UseStatusCodePages(); // 404, 500, etc. Sin cuerpo -> también responden con ProblemDetails

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Mapea la documentación OpenAPI en el entorno de desarrollo
    app.MapScalarApiReference(); // Mapea la referencia de la API Scalar en el entorno de desarrollo
}

app.UseHttpsRedirection();

app.Run();
