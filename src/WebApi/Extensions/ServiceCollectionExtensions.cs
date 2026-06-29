using Application.Abstractions.Persistence;
using Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using WebApi.Configuration;

namespace WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IAppDbContext).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(IAppDbContext).Assembly);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var swaggerOptions = configuration.GetSection(SwaggerOptions.Section).Get<SwaggerOptions>();
            if (swaggerOptions is not null)
            {
                options.SwaggerDoc(swaggerOptions.Version, new OpenApiInfo
                {
                    Title = swaggerOptions.Title,
                    Description = swaggerOptions.Description,
                    Version = swaggerOptions.Version,
                    Contact = swaggerOptions.Contact is not null
                        ? new OpenApiContact { Name = swaggerOptions.Contact.Name, Email = swaggerOptions.Contact.Email }
                        : null,
                    License = swaggerOptions.License is not null
                        ? new OpenApiLicense { Name = swaggerOptions.License.Name, Url = new Uri(swaggerOptions.License.Url) }
                        : null
                });
            }

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
        var rateLimitSection = configuration.GetSection("RateLimiting");
        var permitLimit = rateLimitSection.GetValue<int>("PermitLimit", 100);
        var windowSeconds = rateLimitSection.GetValue<int>("WindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("default", opt =>
            {
                opt.PermitLimit = permitLimit;
                opt.Window = TimeSpan.FromSeconds(windowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedOrigins.Length != 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }
}
