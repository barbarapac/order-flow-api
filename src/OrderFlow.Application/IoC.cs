using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application._Shared;

namespace OrderFlow.Application;

public static class DependencyInjection
{
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });

        services.AddValidatorsFromAssembly(ApplicationAssembly);

        return services;
    }
}
