using Microsoft.EntityFrameworkCore;
using urlshortenerapi.Features.Analytics;
using urlshortenerapi.Features.Links;
using urlshortenerapi.Infraestructure.ErrorHandling;
using urlshortenerapi.Infraestructure.Persistence;

namespace urlshortenerapi.Infraestructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "URL Shortener API";
                    document.Info.Version = "v1";
                    document.Info.Description = "API para acortar URLs y consultar analíticas de clics.";
                    return Task.CompletedTask;
                });
            });

            return services;
        }

        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance =
                        $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                };
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public static IServiceCollection AddApplicationOptions(this IServiceCollection services)
        {
            services.AddOptions<ShortLinkOptions>()
                .BindConfiguration(ShortLinkOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ClickTrackingOptions>()
                .BindConfiguration(ClickTrackingOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'DefaultConnection'. En desarrollo, configúrala en User Secrets.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
