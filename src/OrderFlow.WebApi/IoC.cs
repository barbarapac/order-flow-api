using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using OrderFlow.WebApi._Shared;

namespace OrderFlow.WebApi;

public static class IoC
{
    public static void AddWebApi(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        AddJson(services);
        AddSwagger(services);
        AddCors(services, configuration, environment);
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection(CorsPolicies.AllowedOriginsSection).Get<string[]>();

        if (allowedOrigins is null or { Length: 0 })
        {
            allowedOrigins = environment.IsDevelopment()
                ? CorsPolicies.DefaultAllowedOrigins
                : throw new InvalidOperationException(
                    $"'{CorsPolicies.AllowedOriginsSection}' is not configured.");
        }

        services.AddCors(options =>
            options.AddPolicy(CorsPolicies.Frontend, policy => policy
                .WithOrigins(CorsPolicies.Normalize(allowedOrigins))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetPreflightMaxAge(CorsPolicies.PreflightMaxAge)));
    }

    private static void AddJson(IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderFlow API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "JWT obtained from POST /auth/token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document, null), [] }
            });
        });
    }
}
