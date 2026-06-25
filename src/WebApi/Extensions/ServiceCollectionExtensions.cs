using FluentValidation;
using Shared.Behaviors;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddHealthChecks();

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

        var corsSection = configuration.GetSection("Cors:AllowedOrigins");
        var allowedOrigins = corsSection.Get<string[]>() ?? [];
        if (allowedOrigins.Length != 0)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
        }

        return services;
    }
}
