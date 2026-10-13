using Scalar.AspNetCore;
using urlshortenerapi.Features.Analytics.Endpoints;
using urlshortenerapi.Features.Auth;
using urlshortenerapi.Features.Links.Endpoints;
using urlshortenerapi.Features.Redirect;
using urlshortenerapi.Infraestructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
// Agregando servicios al contenedor de dependencias.
// ApiDocumentation: Agrega servicios para generar documentación OpenAPI (Swagger).
// ErrorHandling: Agrega servicios para manejar errores y excepciones de manera centralizada.
// ApplicationOptions: Agrega servicios para configurar opciones de la aplicación a partir de la configuración (appsettings.json, variables de entorno, etc.).
builder.Services
    .AddApiDocumentation()
    .AddErrorHandling()
    .AddApplicationOptions()
    .AddForwardedHeadersSupport(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddAuth()
    .AddLinksFeature()
    .AddRedirectFeature()
    .AddAnalyticsFeature();

var app = builder.Build();

// Configuración de la aplicación para procesar encabezados de proxy inverso (X-Forwarded-For, X-Forwarded-Proto, etc.)
app.UseForwardedHeaders();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints(); // Mapea los endpoints de autenticación y autorización
app.MapLinkEndpoints(); // Mapea los endpoints relacionados con la funcionalidad de acortamiento de enlaces
app.MapAnalyticsEndpoints(); // Mapea los endpoints relacionados con la funcionalidad de análisis y estadísticas de enlaces
app.MapRedirectEndpoints(); // Mapea los endpoints relacionados con la redirección de enlaces

app.Run();
