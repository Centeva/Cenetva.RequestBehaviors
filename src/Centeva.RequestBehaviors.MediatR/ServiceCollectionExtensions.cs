using Centeva.RequestBehaviors.MediatR.Authorization;
using Centeva.RequestBehaviors.MediatR.FluentValidation;
using FluentValidation;
using MediatR;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for registering MediatR pipeline behaviors and related services with an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all request authorizers and authorization handlers from the specified assemblies,
    /// and registers the RequestAuthorizationBehavior in the MediatR pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatRAuthorization(
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
    /// and registers the RequestAuthorizationBehavior in the MediatR pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatRAuthorization(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped) => services.AddMediatRAuthorization([assembly], lifetime);

    /// <summary>
    /// Adds all FluentValidation validators from the specified assemblies to the service collection,
    /// and registers the ValidationBehavior in the MediatR pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <param name="filter"></param>
    /// <param name="includeInternalTypes"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatRFluentValidation(
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
    /// and registers the ValidationBehavior in the MediatR pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <param name="filter"></param>
    /// <param name="includeInternalTypes"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatRFluentValidation(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        Func<AssemblyScanner.AssemblyScanResult, bool>? filter = null,
        bool includeInternalTypes = false
        ) => AddMediatRFluentValidation(services, [assembly], lifetime, filter, includeInternalTypes);
}
