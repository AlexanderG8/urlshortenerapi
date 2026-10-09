using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Net;
using urlshortenerapi.Features.Analytics;
using urlshortenerapi.Features.Auth;
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

        /// <summary>
        /// Configura la autenticación y autorización de la API.
        /// Ayudará a proteger los endpoints y a manejar la identidad de los usuarios. 
        /// El access token tiene una duración de 30 minutos y el refresh token de 7 días. 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddAuthorization();

            services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
                .AddEntityFrameworkStores<AppDbContext>();

            services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromMinutes(30);
                options.RefreshTokenExpiration = TimeSpan.FromDays(7);
            });

            return services;
        }

        /// <summary>
        /// Configura los servicios relacionados con la funcionalidad de acortamiento de enlaces, 
        /// incluyendo la generación de códigos cortos, validación de alias y validación de URLs de destino.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLinksFeature(this IServiceCollection services)
        {
            services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
            services.AddSingleton<IAliasValidator, AliasValidator>();
            services.AddSingleton<IDestinationUrlValidator, DestinationUrlValidator>();

            return services;
        }
    }
}
