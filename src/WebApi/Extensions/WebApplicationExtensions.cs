using Serilog;
using WebApi.Configuration;
using WebApi.Middleware;

namespace WebApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSerilogRequestLogging();

        var swaggerOptions = app.Configuration.GetSection(SwaggerOptions.Section).Get<SwaggerOptions>();
        if (swaggerOptions?.AllowSwaggerUi == true)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRateLimiter();

        app.UseCors();

        app.UseAuthentication();

        app.UseAuthorization();

        return app;
    }
}
