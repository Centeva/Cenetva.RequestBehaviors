using Centeva.RequestBehaviors.Mediator.Authorization;
using Centeva.RequestBehaviors.Mediator.FluentValidation;
using FluentValidation;
using Mediator;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for registering Mediator pipeline behaviors and related services with an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all request authorizers and authorization handlers from the specified assemblies,
    /// and registers the RequestAuthorizationBehavior in the Mediator pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatorAuthorization(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies, 
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>), lifetime));
        services.AddRequestAuthorizersFromAssemblies(assemblies, lifetime);
        services.AddRequestAuthorizationHandlersFromAssemblies(assemblies, lifetime);

        return services;
    }

    /// <summary>
    /// Adds all request authorizers and authorization handlers from the specified assembly,
    /// and registers the RequestAuthorizationBehavior in the Mediator pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatorAuthorization(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped) 
        => services.AddMediatorAuthorization([assembly], lifetime);

    /// <summary>
    /// Adds all FluentValidation validators from the specified assemblies to the service collection,
    /// and registers the ValidationBehavior in the Mediator pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <param name="filter"></param>
    /// <param name="includeInternalTypes"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatorFluentValidation(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false
        )
    {
        services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>), lifetime));

        services.AddValidatorsFromAssemblies(assemblies, lifetime, filter, includeInternalTypes);

        return services;
    }

    /// <summary>
    /// Adds all FluentValidation validators from the specified assembly to the service collection,
    /// and registers the ValidationBehavior in the Mediator pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <param name="filter"></param>
    /// <param name="includeInternalTypes"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatorFluentValidation(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false
        ) => AddMediatorFluentValidation(services, [assembly], lifetime, filter, includeInternalTypes);
}

