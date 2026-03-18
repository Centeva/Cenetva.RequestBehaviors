using Centeva.RequestBehaviors.Common.Authorization;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for registering request authorizers and authorization handlers from assemblies with an
/// IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all request authorizers from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddRequestAuthorizersFromAssemblies(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        foreach (var assembly in assemblies)
        { 
            services.AddRequestAuthorizersFromAssembly(assembly, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Adds all request authorizers from the specified assembly to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddRequestAuthorizersFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var authorizerType = typeof(IRequestAuthorizer<>);
        GetTypesAssignableTo(assembly, authorizerType).ForEach((type) =>
        {
            foreach (var implementedInterface in type.ImplementedInterfaces)
            {
                if (!implementedInterface.IsGenericType)
                    continue;
                if (implementedInterface.GetGenericTypeDefinition() != authorizerType)
                    continue;

                services.Add(new ServiceDescriptor(implementedInterface, type, lifetime));
            }
        });

        return services;
    }

    /// <summary>
    /// Adds all request authorization handlers from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddRequestAuthorizationHandlersFromAssemblies(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        foreach (var assembly in assemblies)
        {
            services.AddRequestAuthorizationHandlersFromAssembly(assembly, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Adds all request authorization handlers from the specified assembly to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddRequestAuthorizationHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var authHandlerOpenType = typeof(IRequestAuthorizationHandler<>);
        GetTypesAssignableTo(assembly, authHandlerOpenType)
            .ForEach(type =>
            {
                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    if (!implementedInterface.IsGenericType)
                        continue;
                    if (implementedInterface.GetGenericTypeDefinition() != authHandlerOpenType)
                        continue;

                    services.Add(new ServiceDescriptor(implementedInterface, type, lifetime));
                }
            });

        return services;
    }

    private static List<TypeInfo> GetTypesAssignableTo(Assembly assembly, Type compareType)
    {
        return assembly.DefinedTypes.Where(x => x is {IsClass: true, IsAbstract: false}
            && x != compareType
            && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == compareType))?.ToList() ?? [];
    }
}
