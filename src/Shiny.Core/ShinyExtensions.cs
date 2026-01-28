using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shiny;


public static class ShinyExtensions
{
    /// <summary>
    /// Asserts that AccessState is available (or allows restricted)
    /// </summary>
    /// <param name="state"></param>
    /// <param name="message"></param>
    /// <param name="allowRestricted"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Assert(this AccessState state, string? message = null, bool allowRestricted = false)
    {
        if (state == AccessState.Available)
            return;
    
        if (allowRestricted && state == AccessState.Restricted)
            return;
    
        throw new InvalidOperationException(message ?? $"Invalid State " + state);
    }
    
    
     /// <summary>
    /// Checks if a service collection has a service registered for the specified type
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static bool HasService<TService>(this IServiceCollection services)
        => services.HasService(typeof(TService));

    /// <summary>
    /// Checks if a service collection has a service registered for the specified type
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public static bool HasService(this IServiceCollection services, Type serviceType)
        => services.Any(x => x.ServiceType == serviceType);

    /// <summary>
    /// Checks if a service collection has an implementation registered for the specified type
    /// </summary>
    /// <typeparam name="TImpl"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static bool HasImplementation<TImpl>(this IServiceCollection services)
        => services.HasImplementation(typeof(TImpl));

    /// <summary>
    /// Checks if a service collection has an implementation registered for the specified type
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationType"></param>
    /// <returns></returns>
    public static bool HasImplementation(this IServiceCollection services, Type implementationType)
        => services.Any(x => x.ServiceKey == null && x.ImplementationType == implementationType);

    /// <summary>
    /// Lazily resolves a service - helps in prevent resolve loops with delegates/services internal to Shiny
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="required"></param>
    /// <returns></returns>
    public static Lazy<T> GetLazyService<T>(this IServiceProvider services, bool required = false)
        => new(() => required ? services.GetRequiredService<T>() : services.GetService<T>());

    /// <summary>
    /// This will add the implementation for ALL of its interfaces as a singleton
    /// </summary>
    /// <typeparam name="TImpl"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddShinyService<TImpl>(this IServiceCollection services) where TImpl : class
        => services.AddShinyService(typeof(TImpl));

    /// <summary>
    /// This will add the implementation for ALL of its interfaces as a singleton
    /// </summary>
    /// <param name="implementationType"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddShinyService(this IServiceCollection services, Type implementationType)
    {
        var interfaces = implementationType
            .GetInterfaces()
            .Where(x => x != typeof(IDisposable) &&
                       !x.Name.StartsWith("INotify") &&
                       x.Namespace?.StartsWith("System") != true)
            .ToList();

        services.AddSingleton(implementationType);

        foreach (var iface in interfaces)
        {
            services.AddSingleton(iface, sp => sp.GetRequiredService(implementationType));
        }
        return services;
    }

    // /// <summary>
    // /// This will add the implementation for ALL of its interfaces and create a persistent storage binding if INotifyPropertyChanged is implemented
    // /// </summary>
    // /// <typeparam name="TImpl"></typeparam>
    // /// <param name="services"></param>
    // /// <returns></returns>
    // public static IServiceCollection AddShinyService<TImpl>(this IServiceCollection services) where TImpl : class
    //     => services.AddShinyService(typeof(TImpl));

    // /// <summary>
    // /// This will add the implementation for ALL of its interfaces and create a persistent storage binding if INotifyPropertyChanged is implemented
    // /// </summary>
    // /// <param name="implementationType"></param>
    // /// <param name="services"></param>
    // /// <returns></returns>
    // public static IServiceCollection AddShinyService(this IServiceCollection services, Type implementationType)
    // {
    //     var interfaces = implementationType
    //         .GetInterfaces()
    //         .Where(x => x != typeof(IDisposable))
    //         .ToList();
    //
    //     services.AddSingleton(implementationType);
    //
    //     if (interfaces.Any(x => x == typeof(INotifyPropertyChanged)) || interfaces.Any(x => x == typeof(IShinyComponentStartup)))
    //     {
    //         services.AddSingleton(implementationType, services =>
    //         {
    //             var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("ShinyStartup");
    //             var instance = ActivatorUtilities.CreateInstance(services, implementationType);
    //             var fn = implementationType.FullName;
    //
    //             if (instance is INotifyPropertyChanged npc)
    //             {
    //                 logger.LogInformation("Startup Binding for " + fn);
    //
    //                 services
    //                     .GetRequiredService<IObjectStoreBinder>()
    //                     .Bind(npc);
    //             }
    //
    //             if (instance is IShinyComponentStartup startup)
    //             {
    //                 logger.LogInformation("Component Start: " + fn);
    //                 startup.ComponentStart();
    //             }
    //             return instance;
    //         });
    //         interfaces.Remove(typeof(INotifyPropertyChanged));
    //         interfaces.Remove(typeof(IShinyComponentStartup));
    //     }
    //     foreach (var iface in interfaces)
    //     { 
    //         services.AddSingleton(iface, sp => sp.GetRequiredService(implementationType));
    //     }
    //     return services;
    // }


    public static async Task RunDelegates<T>(this IServiceProvider services, Func<T, Task> execute, ILogger logger)
    {
        try
        {
            await services
                .GetServices<T>()
                .RunDelegates(execute, logger)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Could not resolve delegates");
        }
    }


    public static async Task RunDelegates<T>(this IEnumerable<T> services, Func<T, Task> execute, ILogger logger)
    {
        if (services == null)
            return;

        var tasks = services
            .Select(async x =>
            {
                try
                {
                    await execute(x).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing delegate of type " + x!.GetType().FullName);
                }
            })
            .ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}